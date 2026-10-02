using AgitekBackend.Models;

namespace AgitekBackend.Services.Interfaces
{
    public interface IMediaService
    {
        Task ValidateMediaType(Media media);
    }
}
