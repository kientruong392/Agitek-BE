using System.Globalization;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.PcBuilds;
using AgitekBackend.DTOs.Products;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class PcBuildService : IPcBuildService
    {
        private static readonly string[] Categories = ["MAIN", "CPU", "COOL", "RAM", "VGA", "DRIVE", "PSU", "CASE"];
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PcBuildService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsBySpecsAsync(PcComponentType component, PcBuildDTO dto)
        {
            if (!Enum.IsDefined(component)) throw new BadRequestException("Invalid component", "PC_BUILD_INVALID_SELECTION");
            var selected = await LoadSelectionAsync(dto.ProductIds);
            if (component == PcComponentType.Mainboard) selected.Clear();
            else if (!selected.ContainsKey(PcComponentType.Mainboard))
                throw new BadRequestException("Choose a mainboard first", "PC_BUILD_MAINBOARD_REQUIRED");
            selected.Remove(component);
            if (component == PcComponentType.PowerSupply && selected.Count != 7)
                throw new BadRequestException("Choose the other seven components first", "PC_BUILD_INCOMPLETE");

            var candidates = await FilterProducts(component, selected)
                .Include(product => product.Category).Include(product => product.Specifications)
                .Include(product => product.Images).AsSplitQuery()
                .OrderBy(product => product.DiscountPrice ?? product.OriginalPrice).ThenBy(product => product.Id).ToListAsync();
            return _mapper.Map<IEnumerable<ProductDTO>>(candidates.Where(product =>
                HasNumericSpecs(product, component) && FitsSelectedParts(component, product, selected)));
        }

        public async Task<IEnumerable<ProductDTO>> GetCheckoutProductsAsync(int[] productIds)
        {
            if (productIds == null || productIds.Length != 8 || productIds.Any(id => id < 1) || productIds.Distinct().Count() != 8)
                throw new BadRequestException("Choose all eight components", "PC_BUILD_INCOMPLETE");
            var products = await _context.Products.AsNoTracking().Include(product => product.Category)
                .Include(product => product.Specifications).Include(product => product.Images).AsSplitQuery()
                .Where(product => productIds.Contains(product.Id) && product.DeletedAt == null && product.StockQuantity > 0
                    && (product.Status == ProductStatus.InStock || product.Status == ProductStatus.LowStock)).ToListAsync();
            if (products.Count != 8 || products.Any(product => product.Category == null)
                || products.Select(product => product.Category!.Code).Distinct().Count() != 8)
                throw new BadRequestException("A component is unavailable", "PC_BUILD_INCOMPATIBLE");
            return _mapper.Map<IEnumerable<ProductDTO>>(products.OrderBy(product => Array.IndexOf(productIds, product.Id)));
        }

        public async Task<IEnumerable<PcBuildDTO>> GetMyAsync(int userId)
        {
            var builds = await BuildQuery().Where(build => build.UserId == userId)
                .OrderByDescending(build => build.UpdatedAt).ToListAsync();
            return _mapper.Map<IEnumerable<PcBuildDTO>>(builds);
        }

        public async Task<PcBuildDTO> GetByIdAsync(int id, int userId)
        {
            var build = await BuildQuery().FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId)
                ?? throw new NotFoundException("PC configuration not found");
            return _mapper.Map<PcBuildDTO>(build);
        }

        public async Task<PcBuildDTO> CreateAsync(SavePcBuildDTO dto, int userId)
        {
            var products = await SaveProductsAsync(dto);
            if (await _context.PcBuilds.CountAsync(build => build.UserId == userId) >= 10)
                throw new BadRequestException("You can save up to ten PC configurations", "PC_BUILD_LIMIT_REACHED");
            var build = _mapper.Map<PcBuild>(dto);
            build.UserId = userId;
            build.Name = build.Name.Trim();
            build.Products = products.Values.ToList();
            _context.PcBuilds.Add(build);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(build.Id, userId);
        }

        public async Task<PcBuildDTO> UpdateAsync(int id, SavePcBuildDTO dto, int userId)
        {
            var build = await _context.PcBuilds.Include(item => item.Products)
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId)
                ?? throw new NotFoundException("PC configuration not found");
            var products = await SaveProductsAsync(dto);
            _mapper.Map(dto, build);
            build.Name = build.Name.Trim();
            build.UpdatedAt = DateTime.UtcNow;
            foreach (var product in build.Products.Where(product => !products.Values.Any(selected => selected.Id == product.Id)).ToList())
                build.Products.Remove(product);
            foreach (var product in products.Values.Where(product => build.Products.All(existing => existing.Id != product.Id)))
                build.Products.Add(product);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id, userId);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var build = await _context.PcBuilds.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId)
                ?? throw new NotFoundException("PC configuration not found");
            _context.PcBuilds.Remove(build);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Dictionary<PcComponentType, Product>> SaveProductsAsync(SavePcBuildDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > 100)
                throw new BadRequestException("Enter a configuration name", "PC_BUILD_NAME_REQUIRED");
            var products = await LoadSelectionAsync(dto.ProductIds);
            if (!products.ContainsKey(PcComponentType.Mainboard))
                throw new BadRequestException("Choose a mainboard first", "PC_BUILD_MAINBOARD_REQUIRED");
            return products;
        }

        private async Task<Dictionary<PcComponentType, Product>> LoadSelectionAsync(IReadOnlyCollection<int>? productIds)
        {
            if (productIds == null || productIds.Count > 8 || productIds.Any(id => id < 1)
                || productIds.Distinct().Count() != productIds.Count)
                throw new BadRequestException("Invalid PC component selection", "PC_BUILD_INVALID_SELECTION");
            var products = await _context.Products.Include(product => product.Category).Include(product => product.Specifications)
                .Where(product => productIds.Contains(product.Id)).ToListAsync();
            if (products.Count != productIds.Count || products.Any(product => product.Category == null)
                || products.Select(product => product.Category!.Code).Distinct().Count() != products.Count)
                throw new BadRequestException("Invalid PC component selection", "PC_BUILD_INVALID_SELECTION");
            return products.ToDictionary(product => (PcComponentType)Array.IndexOf(Categories, product.Category!.Code));
        }

        private IQueryable<Product> FilterProducts(PcComponentType component, IReadOnlyDictionary<PcComponentType, Product> selected)
        {
            var category = Categories[(int)component];
            var query = _context.Products.AsNoTracking().Where(product => product.Category != null && product.Category.Code == category
                && product.DeletedAt == null && product.StockQuantity > 0
                && (product.Status == ProductStatus.InStock || product.Status == ProductStatus.LowStock));

            void Match(PcComponentType other, string name)
            {
                if (!selected.TryGetValue(other, out var product)) return;
                var values = product.Specifications.Where(spec => spec.SpecName == name).Select(spec => spec.SpecValue).ToArray();
                query = query.Where(candidate => candidate.Specifications.Any(spec => spec.SpecName == name && values.Contains(spec.SpecValue)));
            }

            switch (component)
            {
                case PcComponentType.Cpu: Match(PcComponentType.Mainboard, "Socket"); break;
                case PcComponentType.CpuCooler:
                    Match(selected.ContainsKey(PcComponentType.Cpu) ? PcComponentType.Cpu : PcComponentType.Mainboard, "Socket");
                    break;
                case PcComponentType.Ram:
                    Match(PcComponentType.Mainboard, "RamType");
                    Match(PcComponentType.Mainboard, "MemoryFormFactor");
                    break;
                case PcComponentType.Storage: Match(PcComponentType.Mainboard, "Interface"); break;
                case PcComponentType.Case: Match(PcComponentType.Mainboard, "FormFactor"); break;
            }

            string[] required;
            switch (component)
            {
                case PcComponentType.Mainboard:
                    required = ["Socket", "RamType", "MemoryFormFactor", "Interface", "FormFactor"];
                    break;
                case PcComponentType.Cpu:
                case PcComponentType.CpuCooler:
                    required = ["Socket"];
                    break;
                case PcComponentType.Ram:
                    required = ["RamType", "MemoryFormFactor"];
                    break;
                case PcComponentType.Storage:
                    required = ["Interface"];
                    break;
                case PcComponentType.Case:
                    required = ["FormFactor"];
                    break;
                default:
                    required = [];
                    break;
            }
            foreach (var name in required)
                query = query.Where(product => product.Specifications.Any(spec => spec.SpecName == name && spec.SpecValue != ""));
            return query;
        }

        private static decimal? Number(Product product, string name)
        {
            var values = product.Specifications.Where(spec => spec.SpecName == name).ToArray();
            return values.Length == 1 && decimal.TryParse(values[0].SpecValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
                && value is >= 0 and <= 10000 ? value : null;
        }

        private static bool HasNumericSpecs(Product product, PcComponentType component)
        {
            if (component == PcComponentType.PowerSupply)
                return Number(product, "Watt") is > 0;
            if (!Number(product, "TDP").HasValue)
                return false;
            if (component == PcComponentType.GraphicsCard)
                return Number(product, "VgaLength") is > 0;
            if (component == PcComponentType.Case)
                return Number(product, "MaxVgaLength") is > 0;
            return true;
        }

        private static bool FitsSelectedParts(PcComponentType component, Product candidate,
            IReadOnlyDictionary<PcComponentType, Product> selected)
        {
            if (component == PcComponentType.GraphicsCard && selected.TryGetValue(PcComponentType.Case, out var pcCase))
                return Number(candidate, "VgaLength") <= Number(pcCase, "MaxVgaLength");
            if (component == PcComponentType.Case && selected.TryGetValue(PcComponentType.GraphicsCard, out var gpu))
                return Number(gpu, "VgaLength") <= Number(candidate, "MaxVgaLength");
            if (component == PcComponentType.PowerSupply)
            {
                if (selected.Values.Any(product => Number(product, "TDP") == null)) return false;
                var requiredWatt = (selected.Values.Sum(product => Number(product, "TDP")!.Value) + 50m) * 1.3m;
                return Number(candidate, "Watt") >= requiredWatt;
            }
            return true;
        }

        private IQueryable<PcBuild> BuildQuery()
        {
            return _context.PcBuilds.AsNoTracking()
                .Include(build => build.Products).ThenInclude(product => product.Category)
                .Include(build => build.Products).ThenInclude(product => product.Images)
                .Include(build => build.Products).ThenInclude(product => product.Specifications).AsSplitQuery();
        }
    }
}
