using System.Text;

namespace portfolio.web.Services
{
    public class FileStorage : IFileStorage
    {
        private const long MaxImageBytes = 5 * 1024 * 1024;
        private const long MaxDocumentBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx"
        };

        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp", "application/octet-stream"
        };

        private static readonly HashSet<string> AllowedDocumentContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/octet-stream"
        };

        private readonly IWebHostEnvironment _environment;

        public FileStorage(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveAsync(
            IFormFile file,
            string folder,
            FileUploadKind kind,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new FileValidationException("The selected file is empty.");
            }

            var maxBytes = kind == FileUploadKind.Image ? MaxImageBytes : MaxDocumentBytes;
            if (file.Length > maxBytes)
            {
                throw new FileValidationException(
                    $"The file is too large. The maximum size is {maxBytes / (1024 * 1024)} MB.");
            }

            var extension = Path.GetExtension(file.FileName);
            var allowedExtensions = kind == FileUploadKind.Image
                ? AllowedImageExtensions
                : AllowedDocumentExtensions;

            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            {
                var allowed = string.Join(", ", allowedExtensions.OrderBy(e => e));
                throw new FileValidationException($"Unsupported file type. Allowed types: {allowed}.");
            }

            var allowedContentTypes = kind == FileUploadKind.Image
                ? AllowedImageContentTypes
                : AllowedDocumentContentTypes;

            if (!string.IsNullOrWhiteSpace(file.ContentType) && !allowedContentTypes.Contains(file.ContentType))
            {
                throw new FileValidationException("The file content type does not match its extension.");
            }

            await using (var stream = file.OpenReadStream())
            {
                if (!await HasValidSignatureAsync(stream, extension, cancellationToken))
                {
                    throw new FileValidationException("The file content does not match its extension.");
                }
            }

            var root = EnsureWithinWebRoot(folder);
            Directory.CreateDirectory(root);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var fullPath = Path.Combine(root, fileName);

            await using (var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await using (var source = file.OpenReadStream())
            {
                await source.CopyToAsync(target, cancellationToken);
            }

            var publicFolder = folder.Replace('\\', '/').Trim('/');
            return $"/{publicFolder}/{fileName}";
        }

        public void Delete(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            // Never delete application assets such as the bundled project fallback image.
            if (relativePath.Contains("default-project", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relative = relativePath.TrimStart('/', '~').Replace('/', Path.DirectorySeparatorChar);
            if (relative.Contains("..", StringComparison.Ordinal))
            {
                return;
            }

            var webRoot = Path.GetFullPath(_environment.WebRootPath);
            var fullPath = Path.GetFullPath(Path.Combine(webRoot, relative));

            if (!fullPath.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private string EnsureWithinWebRoot(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new FileValidationException("A destination folder is required.");
            }

            var webRoot = Path.GetFullPath(_environment.WebRootPath);
            var target = Path.GetFullPath(Path.Combine(webRoot, folder));

            if (!target.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new FileValidationException("The destination folder is not valid.");
            }

            return target;
        }

        private static async Task<bool> HasValidSignatureAsync(
            Stream stream,
            string extension,
            CancellationToken cancellationToken)
        {
            var header = new byte[12];
            var read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
            if (read < 4)
            {
                return false;
            }

            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => StartsWith(header, 0xFF, 0xD8, 0xFF),
                ".png" => StartsWith(header, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
                ".gif" => StartsWith(header, 0x47, 0x49, 0x46, 0x38),
                ".webp" => HasAscii(header, 0, "RIFF") && HasAscii(header, 8, "WEBP"),
                ".pdf" => HasAscii(header, 0, "%PDF"),
                ".doc" => StartsWith(header, 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1),
                ".docx" => StartsWith(header, 0x50, 0x4B, 0x03, 0x04),
                _ => false
            };
        }

        private static bool StartsWith(byte[] header, params byte[] signature)
        {
            if (header.Length < signature.Length)
            {
                return false;
            }

            for (var i = 0; i < signature.Length; i++)
            {
                if (header[i] != signature[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasAscii(byte[] header, int offset, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (header.Length < offset + bytes.Length)
            {
                return false;
            }

            for (var i = 0; i < bytes.Length; i++)
            {
                if (header[offset + i] != bytes[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
