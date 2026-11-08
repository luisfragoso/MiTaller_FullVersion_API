using SkiaSharp;

namespace MiTaller.Services
{
    public static class ImageResizer
    {
        public const int FullMaxSide = 1280;
        public const int ThumbMaxSide = 256;

        /// <summary>
        /// Re-encodes any image as JPEG with its longest side at most <paramref name="maxSide"/>,
        /// applying the EXIF rotation (re-encoding drops EXIF). Returns null if the bytes are not an image.
        /// </summary>
        public static byte[]? ToJpeg(byte[] source, int maxSide, int quality = 80)
        {
            using var codec = SKCodec.Create(new MemoryStream(source));
            if (codec == null) return null;
            using var decoded = SKBitmap.Decode(codec);
            if (decoded == null) return null;

            var origin = codec.EncodedOrigin;
            var swap = origin is SKEncodedOrigin.RightTop or SKEncodedOrigin.LeftBottom
                or SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightBottom;
            int srcW = swap ? decoded.Height : decoded.Width;
            int srcH = swap ? decoded.Width : decoded.Height;
            var scale = Math.Min(1.0, (double)maxSide / Math.Max(srcW, srcH));
            int w = Math.Max(1, (int)Math.Round(srcW * scale));
            int h = Math.Max(1, (int)Math.Round(srcH * scale));

            using var surface = SKSurface.Create(new SKImageInfo(w, h));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White); // JPEG has no alpha: transparent PNG areas become white, not black
            canvas.Scale((float)scale);
            ApplyOrientation(canvas, origin, decoded.Width, decoded.Height);
            using var paint = new SKPaint { IsAntialias = true };
            using var image = SKImage.FromBitmap(decoded);
            canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKCubicResampler.Mitchell), paint);

            using var snapshot = surface.Snapshot();
            using var data = snapshot.Encode(SKEncodedImageFormat.Jpeg, quality);
            return data.ToArray();
        }

        // Maps the stored pixels (w x h) onto the upright output: x' = ScaleX*x + SkewX*y + TransX, y' = SkewY*x + ScaleY*y + TransY
        private static void ApplyOrientation(SKCanvas c, SKEncodedOrigin origin, int w, int h)
        {
            SKMatrix? m = origin switch
            {
                SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
                SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
                SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
                SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
                SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
                _ => null,
            };
            if (m is SKMatrix matrix) c.Concat(in matrix);
        }
    }
}
