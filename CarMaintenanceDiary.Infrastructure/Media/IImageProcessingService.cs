namespace CarMaintenanceDiary.Infrastructure.Media
{
    public interface IImageProcessingService
    {
        /// <summary>
        /// Resizes/re-encodes the given image bytes according to <paramref name="options"/>.
        /// If both Width and Height are null, the original bytes and content type are returned unchanged (fast path).
        /// </summary>
        Task<(byte[] Data, string ContentType)> ProcessAsync(byte[] sourceData, string? sourceContentType, ImageProcessingOptions options);
    }
}
