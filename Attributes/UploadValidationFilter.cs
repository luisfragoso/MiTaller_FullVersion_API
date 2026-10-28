using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Text;

namespace MiTaller.Attributes
{
    /// <summary>
    /// Rechaza cualquier archivo subido (argumento o propiedad de un DTO) que pase de MaxBytes
    /// o cuyo contenido real no sea imagen o PDF; la extensión y el Content-Type no prueban nada.
    /// </summary>
    public class UploadValidationFilter : IAsyncActionFilter
    {
        public const long MaxBytes = 10 * 1024 * 1024;

        private static readonly string[] HeifBrands = { "heic", "heix", "hevc", "heim", "heis", "mif1", "msf1", "heif", "avif" };

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var file in context.ActionArguments.Values.SelectMany(FilesIn))
            {
                if (file.Length > MaxBytes)
                {
                    context.Result = new BadRequestObjectResult("file-too-large");
                    return;
                }
                if (file.Length > 0 && !await IsAllowedAsync(file))
                {
                    context.Result = new BadRequestObjectResult("invalid-file-type");
                    return;
                }
            }

            await next();
        }

        private static IEnumerable<IFormFile> FilesIn(object? value) => value switch
        {
            null or string => Enumerable.Empty<IFormFile>(),
            IFormFile file => new[] { file },
            IEnumerable<IFormFile> files => files,
            IEnumerable => Enumerable.Empty<IFormFile>(),
            _ when value.GetType().IsClass => value.GetType().GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0
                    && (typeof(IFormFile).IsAssignableFrom(p.PropertyType)
                        || typeof(IEnumerable<IFormFile>).IsAssignableFrom(p.PropertyType)))
                .SelectMany(p => FilesIn(p.GetValue(value))),
            _ => Enumerable.Empty<IFormFile>(),
        };

        private static async Task<bool> IsAllowedAsync(IFormFile file)
        {
            var header = new byte[12];
            await using var stream = file.OpenReadStream();
            var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
            if (read < 4) return false;

            bool StartsWith(params byte[] signature) => header.Take(signature.Length).SequenceEqual(signature);
            string Ascii(int offset, int length) => read >= offset + length ? Encoding.ASCII.GetString(header, offset, length) : "";

            return StartsWith(0xFF, 0xD8, 0xFF)                                  // JPEG
                || StartsWith(0x89, 0x50, 0x4E, 0x47)                            // PNG
                || Ascii(0, 4) == "GIF8"
                || (Ascii(0, 4) == "RIFF" && Ascii(8, 4) == "WEBP")
                || Ascii(0, 2) == "BM"
                || (Ascii(4, 4) == "ftyp" && HeifBrands.Contains(Ascii(8, 4)))    // HEIC/HEIF (iPhone)
                || Ascii(0, 4) == "%PDF";
        }
    }
}
