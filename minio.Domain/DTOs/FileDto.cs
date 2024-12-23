using System;

namespace minio.Domain.DTOs
{
    public class FileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public string CreatedAt { get; set; }
        public string Url { get; set; }
    }
}

