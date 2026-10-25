using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgitekBackend.Common;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Products;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartComponents.LocalEmbeddings;

namespace AgitekBackend.Services
{
    public class SemanticSearchService : ISemanticSearchService
    {
        private const int MaximumQueryLength = 500;
        private const float MinimumSimilarity = 0.18f;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly LocalEmbedder _embedder;

        public SemanticSearchService(AppDbContext context, IMapper mapper, LocalEmbedder embedder)
        {
            _context = context;
            _mapper = mapper;
            _embedder = embedder;
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsAsync(string query, int maxResults, CurrentUser? currentUser)
        {
            var normalizedQuery = NormalizeQuery(query);
            if (normalizedQuery.Length < 2)
                return [];

            var queryVector = await GetOrCreateQueryVectorAsync(normalizedQuery);
            var embeddingsQuery = _context.ProductEmbeddings
                .AsNoTracking()
                .Include(embedding => embedding.Product)
                    .ThenInclude(product => product.Images)
                .AsQueryable();

            embeddingsQuery = ApplyVisibilityFilter(embeddingsQuery, currentUser);

            var embeddings = await embeddingsQuery.ToListAsync();
            var matchingProducts = embeddings
                .Select(embedding => new
                {
                    embedding.Product,
                    Score = CalculateCosineSimilarity(queryVector, DeserializeVector(embedding.VectorJson))
                })
                .Where(result => result.Score >= MinimumSimilarity)
                .OrderByDescending(result => result.Score)
                .ThenByDescending(result => result.Product.AverageRating)
                .ThenByDescending(result => result.Product.SoldQuantity)
                .Take(Math.Clamp(maxResults, 1, 24))
                .Select(result => result.Product)
                .ToList();

            return _mapper.Map<IEnumerable<ProductDTO>>(matchingProducts);
        }

        public async Task SynchronizeProductAsync(int productId)
        {
            var product = await _context.Products
                .Include(item => item.Brand)
                .Include(item => item.Category)
                .Include(item => item.Specifications)
                .FirstOrDefaultAsync(item => item.Id == productId);

            var existingEmbedding = await _context.ProductEmbeddings
                .FirstOrDefaultAsync(embedding => embedding.ProductId == productId);

            if (product == null)
            {
                if (existingEmbedding != null)
                {
                    _context.ProductEmbeddings.Remove(existingEmbedding);
                    await _context.SaveChangesAsync();
                }

                return;
            }

            var sourceText = CreateProductSourceText(product);
            var sourceHash = CreateHash(sourceText);
            if (existingEmbedding?.SourceHash == sourceHash && !string.IsNullOrWhiteSpace(existingEmbedding.VectorJson))
                return;

            var vector = CreateVector(sourceText);
            var now = DateTime.UtcNow;

            if (existingEmbedding == null)
            {
                _context.ProductEmbeddings.Add(new ProductEmbedding
                {
                    ProductId = product.Id,
                    SourceHash = sourceHash,
                    VectorJson = JsonSerializer.Serialize(vector, JsonOptions),
                    Dimension = vector.Length,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existingEmbedding.SourceHash = sourceHash;
                existingEmbedding.VectorJson = JsonSerializer.Serialize(vector, JsonOptions);
                existingEmbedding.Dimension = vector.Length;
                existingEmbedding.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SynchronizeAllProductsAsync(CancellationToken cancellationToken = default)
        {
            var productIds = await _context.Products
                .AsNoTracking()
                .Select(product => product.Id)
                .ToListAsync(cancellationToken);

            foreach (var productId in productIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await SynchronizeProductAsync(productId);
            }
        }

        private async Task<float[]> GetOrCreateQueryVectorAsync(string normalizedQuery)
        {
            var queryHash = CreateHash(normalizedQuery);
            var cachedVector = await _context.SemanticSearchCaches
                .FirstOrDefaultAsync(cache => cache.QueryHash == queryHash);

            if (cachedVector != null)
            {
                var vector = DeserializeVector(cachedVector.VectorJson);
                if (vector.Length > 0)
                    return vector;

                var regeneratedVector = CreateVector(normalizedQuery);
                cachedVector.VectorJson = JsonSerializer.Serialize(regeneratedVector, JsonOptions);
                cachedVector.Dimension = regeneratedVector.Length;
                cachedVector.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return regeneratedVector;
            }

            var createdVector = CreateVector(normalizedQuery);
            var newCacheEntry = new SemanticSearchCache
            {
                QueryHash = queryHash,
                NormalizedQuery = normalizedQuery,
                VectorJson = JsonSerializer.Serialize(createdVector, JsonOptions),
                Dimension = createdVector.Length,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.SemanticSearchCaches.Add(newCacheEntry);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _context.Entry(newCacheEntry).State = EntityState.Detached;
                var concurrentCacheEntry = await _context.SemanticSearchCaches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cache => cache.QueryHash == queryHash);
                var concurrentVector = concurrentCacheEntry == null ? [] : DeserializeVector(concurrentCacheEntry.VectorJson);

                if (concurrentVector.Length == 0)
                    throw;

                return concurrentVector;
            }

            return createdVector;
        }

        private static IQueryable<ProductEmbedding> ApplyVisibilityFilter(
            IQueryable<ProductEmbedding> query,
            CurrentUser? currentUser)
        {
            var userRole = currentUser?.Role;
            if (userRole == RoleName.SuperAdmin || userRole == RoleName.Admin || userRole == RoleName.Manager)
                return query;

            if (userRole == RoleName.Staff)
                return query.Where(embedding => embedding.Product.Status != ProductStatus.Deleted);

            return query.Where(embedding => embedding.Product.Status != ProductStatus.Pending
                && embedding.Product.Status != ProductStatus.Deleted
                && embedding.Product.Status != ProductStatus.Hidden);
        }

        private float[] CreateVector(string sourceText)
        {
            return _embedder.Embed(sourceText).Values.ToArray();
        }

        private static string CreateProductSourceText(Product product)
        {
            var specifications = product.Specifications
                .OrderBy(specification => specification.SpecName)
                .Select(specification => $"{specification.SpecName}: {specification.SpecValue}");

            return string.Join(Environment.NewLine,
            [
                $"Tên sản phẩm: {product.ProductName}",
                $"Mã sản phẩm: {product.SKU}",
                $"Thương hiệu: {product.Brand?.Name ?? string.Empty}",
                $"Danh mục: {product.Category?.CategoryName ?? string.Empty}",
                $"Mô tả: {product.Description ?? string.Empty}",
                $"Thông số: {string.Join("; ", specifications)}"
            ]);
        }

        private static string NormalizeQuery(string query)
        {
            var normalized = string.Join(' ', query.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return normalized.Length > MaximumQueryLength ? normalized[..MaximumQueryLength] : normalized;
        }

        private static string CreateHash(string value)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
        }

        private static float[] DeserializeVector(string vectorJson)
        {
            try
            {
                return JsonSerializer.Deserialize<float[]>(vectorJson, JsonOptions) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static float CalculateCosineSimilarity(IReadOnlyList<float> firstVector, IReadOnlyList<float> secondVector)
        {
            if (firstVector.Count == 0 || firstVector.Count != secondVector.Count)
                return 0;

            double dotProduct = 0;
            double firstMagnitude = 0;
            double secondMagnitude = 0;

            for (var index = 0; index < firstVector.Count; index++)
            {
                dotProduct += firstVector[index] * secondVector[index];
                firstMagnitude += firstVector[index] * firstVector[index];
                secondMagnitude += secondVector[index] * secondVector[index];
            }

            if (firstMagnitude == 0 || secondMagnitude == 0)
                return 0;

            return (float)(dotProduct / (Math.Sqrt(firstMagnitude) * Math.Sqrt(secondMagnitude)));
        }
    }
}
