namespace portfolio.web.Services
{
    public enum FileUploadKind
    {
        Image,
        Document
    }

    public interface IFileStorage
    {
        /// <summary>
        /// Validates and stores an uploaded file under wwwroot, returning its public URL.
        /// </summary>
        Task<string> SaveAsync(
            IFormFile file,
            string folder,
            FileUploadKind kind,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a previously stored file. Paths outside wwwroot are ignored.
        /// </summary>
        void Delete(string? relativePath);
    }

    public class FileValidationException : Exception
    {
        public FileValidationException(string message) : base(message)
        {
        }
    }
}
