using System.Text;
using AgitekBackend.Data;
using AgitekBackend.Middlewares;
using AgitekBackend.Services;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AgitekConnection")
    ?? throw new InvalidOperationException("Connection string 'AgitekConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

var redisConnectionString = builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException("Redis:ConnectionString not configured.");
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

var jwtKey    = builder.Configuration["JwtSettings:SecretKey"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAud    = builder.Configuration["JwtSettings:Audience"]!;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAud,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    success    = false,
                    statusCode = 401,
                    errorCode  = "UNAUTHORIZED",
                    message    = "Authentication required. Please provide a valid Bearer token.",
                    timestamp  = DateTime.UtcNow,
                });
            },
            OnForbidden = async ctx =>
            {
                ctx.Response.StatusCode  = 403;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    success    = false,
                    statusCode = 403,
                    errorCode  = "FORBIDDEN",
                    message    = "You do not have permission to access this resource.",
                    timestamp  = DateTime.UtcNow,
                });
            },
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId     = builder.Configuration["GoogleAuth:ClientId"]!;
        options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"]!;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var hasBindingErrors = context.ModelState.Any(e =>
            e.Key.StartsWith("$") ||
            e.Value?.Errors.Any(err =>
                err.ErrorMessage.Contains("The JSON value could not be converted") ||
                err.ErrorMessage.Contains("is invalid after a value") ||
                err.Exception != null) == true);

        if (hasBindingErrors)
        {
            var response = new
            {
                success    = false,
                statusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
                errorCode  = "INVALID_REQUEST_BODY",
                message    = "Request body contains invalid data. Please check the field types and format.",
                timestamp  = DateTime.Now
            };
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        }

        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => string.IsNullOrEmpty(kvp.Key)
                    ? kvp.Key
                    : char.ToLowerInvariant(kvp.Key[0]) + kvp.Key.Substring(1),
                kvp => kvp.Value!.Errors.First().ErrorMessage
            );

        var validationResponse = new
        {
            success    = false,
            statusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
            errorCode  = "VALIDATION_ERROR",
            message    = "Validation failed",
            errors     = errors,
            timestamp  = DateTime.Now
        };

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(validationResponse);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
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

app.UseMiddleware<GlobalExceptionMiddleware>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();
