namespace CarMaintenanceDiary.Infrastructure.Media
{
    /// <summary>
    /// Options controlling on-the-fly image resizing/encoding for image endpoints (vehicle photos, fuel photos, maintenance documents).
    /// </summary>
    public sealed class ImageProcessingOptions
    {
        public int? Width { get; init; }
        public int? Height { get; init; }
        public string Mode { get; init; } = "crop";
        public int Dpr { get; init; } = 1;
        public string? Format { get; init; }
        public int Quality { get; init; } = 82;
        public float Sharpen { get; init; } = 0.8f;
    }
}
