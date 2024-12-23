using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Args;
using minio.Domain.entity;
using minio.Domain.Enumerations;
using Minio.Exceptions;
using Minio;
using System.IO;
using System.Threading.Tasks;

public class FileRetrievalService 
{
    private readonly MinioClient _minioClient;
    private readonly ILogger<FileRetrievalService> _logger;

    public FileRetrievalService(MinioClient minioClient, ILogger<FileRetrievalService> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task<IActionResult> RetrieveImageResource(string objectName, string bucketName, string contentType)
    {
        try
        {
            var memoryStream = new MemoryStream();

            // Minio'dan faylni olish
            await _minioClient.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream))
            );

            var data = memoryStream.ToArray();

            // Faylni HTTP Response sifatida qaytarish
            return new FileContentResult(data, contentType);
        }
        catch (MinioException e)
        {
            _logger.LogError(e, $"Error retrieving image resource from Minio: {e.Message}");
            throw new IOException("Error retrieving file from Minio", e);
        }
    }

    public async Task<IActionResult> RetrieveFileResource(FileEntity fileEntity)
    {
        if (fileEntity.SaveMode == SaveMode.SYSTEM_SERVER || fileEntity.SaveMode == null)
        {
            var filePath = fileEntity.Path;
            var fullPath = Path.IsPathFullyQualified(filePath) ? filePath : Path.GetFullPath(filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found", fullPath);

            var data = await File.ReadAllBytesAsync(fullPath);

            return new FileContentResult(data, fileEntity.ContentType)
            {
                FileDownloadName = fileEntity.GetFileName()
            };
        }
        else if (fileEntity.SaveMode == SaveMode.MINIO)
        {
            return await RetrieveImageResource(fileEntity.ObjectName, fileEntity.BucketName, fileEntity.ContentType);
        }

        throw new InvalidOperationException("Invalid SaveMode value");
    }
}
