using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;

namespace AgitekBackend.Services
{
    public class MediaService : IMediaService
    {
        private static readonly string[] VideoExtensions =
        {
            ".mp4", ".mov", ".avi", ".mkv", ".webm", ".m4v",
            ".wmv", ".flv", ".mpeg", ".mpg", ".3gp"
        };

        private readonly AppDbContext _context;

        public MediaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ValidateMediaType(Media media)
        {
            ValidateMediaOwner(media);

            media.MediaType = IsVideoUrl(media.MediaUrl)
                ? MediaType.Video
                : MediaType.Image;

            await _context.SaveChangesAsync();
        }

        private static void ValidateMediaOwner(Media media)
        {
            var ownerIdCount = new[]
            {
                media.ReviewId,
                media.WarrantyId,
                media.OrderId,
                media.RefundOrderId,
                media.RefundProductId
            }.Count(id => id.HasValue);

            var hasRefundPair = media.RefundOrderId.HasValue && media.RefundProductId.HasValue;

            if (media.RefundOrderId.HasValue != media.RefundProductId.HasValue ||
                ownerIdCount > 2 ||
                (ownerIdCount == 2 && !hasRefundPair))
            {
                throw new BadRequestException("Media can only be associated with one resource");
            }
        }

        private static bool IsVideoUrl(string mediaUrl)
        {
            if (mediaUrl.Contains("/video/upload/", StringComparison.OrdinalIgnoreCase))
                return true;

            var urlWithoutQuery = mediaUrl.Split('?', '#')[0];
            return VideoExtensions.Any(extension =>
                urlWithoutQuery.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
