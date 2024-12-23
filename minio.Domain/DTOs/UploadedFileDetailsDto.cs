namespace minio.Domain.DTOs
{
    public class UploadedFileDetailsDto
    {
        public string Etag { get; set; }
        public string Path { get; set; }

        public UploadedFileDetailsDto() { }

        public UploadedFileDetailsDto(string etag, string path)
        {
            Etag = etag;
            Path = path;
        }
    }
}

