namespace minio.Application.Core.Exceptions
{
    public class FileNotFoundException : Exception
    {
        public FileNotFoundException(string message) : base(message)
        {
        }
    }
}
