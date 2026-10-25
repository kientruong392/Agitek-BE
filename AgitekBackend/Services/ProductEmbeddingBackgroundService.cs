using AgitekBackend.Services.Interfaces;

namespace AgitekBackend.Services
{
    public class ProductEmbeddingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProductEmbeddingBackgroundService> _logger;

        public ProductEmbeddingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ProductEmbeddingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                var semanticSearchService = scope.ServiceProvider.GetRequiredService<ISemanticSearchService>();
                await semanticSearchService.SynchronizeAllProductsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Application shutdown is an expected cancellation path.
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not synchronize product embeddings on application startup");
            }
        }
    }
}
