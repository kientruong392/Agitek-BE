namespace AgitekBackend.Services.Interfaces
{
    public interface ISePayService
    {
        Task ProcessWebhookAsync(byte[] rawPayload, string? timestamp, string? signature);
    }
}
