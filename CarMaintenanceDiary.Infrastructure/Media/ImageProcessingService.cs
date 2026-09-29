using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CarMaintenanceDiary.Infrastructure.Media
{
    public class ImageProcessingService : IImageProcessingService
    {
        public async Task<(byte[] Data, string ContentType)> ProcessAsync(byte[] sourceData, string? sourceContentType, ImageProcessingOptions options)
        {
            var contentType = sourceContentType ?? "application/octet-stream";

            // Fast path: no resize requested, return original bytes.
            if (options.Width is null || options.Height is null)
                return (sourceData, contentType);

            using var image = Image.Load(sourceData);

            var dpr = Math.Max(1, options.Dpr);
            var targetSize = new Size(
                Math.Max(1, options.Width.Value * dpr),
                Math.Max(1, options.Height.Value * dpr));

            var resizeMode = options.Mode?.ToLowerInvariant() switch
            {
                "pad" => ResizeMode.Pad,
                "max" => ResizeMode.Max,
                _ => ResizeMode.Crop
            };

            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Size = targetSize,
                    Mode = resizeMode,
                    Sampler = KnownResamplers.Lanczos3
                })
                .GaussianSharpen(Math.Max(0f, options.Sharpen)));

            var quality = Math.Clamp(options.Quality, 1, 100);

            var inferred = contentType.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" => "jpeg",
                "image/png" => "png",
                "image/webp" => "webp",
                _ => null
            };
            var chosen = (options.Format ?? inferred ?? "webp").ToLowerInvariant();

            IImageEncoder encoder = chosen switch
            {
                "jpeg" or "jpg" => new JpegEncoder { Quality = quality },
                "png" => new PngEncoder(),
                _ => new WebpEncoder { Quality = quality }
            };
            var outContentType = chosen switch
            {
                "jpeg" or "jpg" => "image/jpeg",
                "png" => "image/png",
                _ => "image/webp"
            };

            using var ms = new MemoryStream();
            await image.SaveAsync(ms, encoder);
            return (ms.ToArray(), outContentType);
        }
    }
}
