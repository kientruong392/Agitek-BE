using AgitekBackend.Data;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AgitekConnection") ?? throw new InvalidOperationException("Connection string 'AgitekConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// Register Services
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IUserService, AgitekBackend.Services.UserService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IProductService, AgitekBackend.Services.ProductService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IOrderService, AgitekBackend.Services.OrderService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IBrandService, AgitekBackend.Services.BrandService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.ICategoryService, AgitekBackend.Services.CategoryService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IReviewService, AgitekBackend.Services.ReviewService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IVoucherService, AgitekBackend.Services.VoucherService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IWarrantyService, AgitekBackend.Services.WarrantyService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IAddressService, AgitekBackend.Services.AddressService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IProfileService, AgitekBackend.Services.ProfileService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IOrderDetailService, AgitekBackend.Services.OrderDetailService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IOrderTrackingLogService, AgitekBackend.Services.OrderTrackingLogService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IProductImageService, AgitekBackend.Services.ProductImageService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IProductSpecificationService, AgitekBackend.Services.ProductSpecificationService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IMediaService, AgitekBackend.Services.MediaService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.IAuditLogService, AgitekBackend.Services.AuditLogService>();
builder.Services.AddScoped<AgitekBackend.Services.Interfaces.ILocationCoordinatesService, AgitekBackend.Services.LocationCoordinatesService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agitek API V1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
