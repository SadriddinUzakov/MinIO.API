using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.Exceptions;
using minio.Domain.DTOs;
using minio.Domain.entity;
using minio.Domain.Enumerations;
using minio.Application.Core.Exceptions;
using minio.Infrastructure.Repositories;
using Minio.DataModel.Args;
using minio.Domain.Core.Utilities;
using minio.Infrastructure.Services.file;
using System.Text;
using FluentMigrator;
using FileNotFoundException = minio.Application.Core.Exceptions.FileNotFoundException;

namespace minio.Infrastructure.Services.miniO
{
    [Profile("minio")]
    public class minIOServiceImpl : IFileService
    {
        private readonly MinioClient _minioClient;
        private readonly IFileRepository _fileRepository;
        private readonly FileRetrievalService _fileRetrievalService;
        private readonly ILogger<minIOServiceImpl> _logger;
        private readonly string _defaultUploadPath = "{YYYY}/{MM}/{DD}/";
        private readonly string _defaultBucketName = "default-bucket";

        public minIOServiceImpl(MinioClient minioClient, IFileRepository fileRepository, FileRetrievalService fileRetrievalService, ILogger<minIOServiceImpl> logger)
        {
            _minioClient = minioClient;
            _fileRepository = fileRepository;
            _fileRetrievalService = fileRetrievalService;
            _logger = logger;
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
                var bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs);
                if (!bucketExists)
                {
                    var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                    await _minioClient.MakeBucketAsync(makeBucketArgs);
                }
            }
            catch (Exception e)
            {
                throw new MinioException($"Error creating bucket: {bucketName}; {e.Message}");
            }
        }

        public async Task<UploadedFileDetailsDto> UploadFileAsync(string bucketName, string objectName, Stream inputStream)
        {
            try
            {
                _logger.LogInformation("Bucket Name: {BucketName}, Object Name: {ObjectName}", bucketName, objectName);
                await CreateBucketAsync(bucketName);
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(inputStream)
                    .WithObjectSize(inputStream.Length);
                await _minioClient.PutObjectAsync(putObjectArgs);

                var statObjectArgs = new StatObjectArgs().WithBucket(bucketName).WithObject(objectName);
                var statObject = await _minioClient.StatObjectAsync(statObjectArgs);
                return new UploadedFileDetailsDto(statObject.ETag, objectName);
            }
            catch (Exception e)
            {
                throw new MinioException($"Unexpected error happened: {e.Message}");
            }
        }

        public async Task<FileDto> SaveFile(IFormFile file, SaveType type, string tenantId, string module)
        {
            if (file.Length == 0)
                throw new FileStorageException("File is empty");

            var originalFilename = Path.GetFileName(file.FileName);
            if (originalFilename.Contains(".."))
                throw new FileStorageException("Filename contains invalid path sequence " + originalFilename);

            try
            {
                var fileEntity = new FileEntity
                {
                    Name = Path.GetFileNameWithoutExtension(originalFilename),
                    Extension = Path.GetExtension(originalFilename),
                    Size = file.Length,
                    ContentType = file.ContentType,
                    CreatedAt = DateTime.UtcNow
                };
                await _fileRepository.AddAsync(fileEntity);

                var uniqueKey = MD5Decode.Md5Decode(fileEntity.Id);
                uniqueKey = $"{uniqueKey}.{Path.GetExtension(file.FileName)}";
                var wholePath = new StringBuilder(type == SaveType.PRIVATE ? SaveType.PRIVATE.ToString() : SaveType.PUBLIC.ToString()).Append("/");
                var realWholePath = PrepareUploadPath(wholePath.Append(tenantId).Append("/").Append(module.ToUpper()).Append("/").Append(_defaultUploadPath).ToString());
                var objectName = realWholePath.EndsWith("/") ? $"{realWholePath}{uniqueKey}" : $"{realWholePath}/{uniqueKey}";

                using (var inputStream = file.OpenReadStream())
                {
                    var res = await UploadFileAsync(_defaultBucketName, objectName, inputStream);
                    fileEntity.SaveMode = SaveMode.MINIO;
                    fileEntity.Etag = res.Etag;
                    fileEntity.ObjectName = res.Path;
                    fileEntity.BucketName = _defaultBucketName;
                    fileEntity.Path = res.Path;
                    fileEntity.UniqueKey = uniqueKey;

                    await _fileRepository.UpdateAsync(fileEntity);
                }

                return new FileDto
                {
                    Id = fileEntity.Id,
                    Name = fileEntity.Name,
                    Size = fileEntity.Size,
                    Extension = fileEntity.Extension,
                    ContentType = fileEntity.ContentType,
                    CreatedAt = fileEntity.CreatedAt.ToString("o"),
                    Url = $"/api/file/view-image/{type.ToString().ToLower()}/{uniqueKey}"
                };
            }
            catch (Exception e)
            {
                var msg = $"Could not store file {originalFilename}. Please try again!";
                _logger.LogError(e, msg);
                throw new FileStorageException(msg);
            }
        }

        public string PrepareUploadPath(string uploadPath)
        {
            if (string.IsNullOrEmpty(uploadPath))
                uploadPath = _defaultUploadPath;

            var now = DateTime.UtcNow;

            return uploadPath
                .Replace("{YYYY}", now.Year.ToString())
                .Replace("{MM}", now.Month.ToString("D2"))
                .Replace("{DD}", now.Day.ToString("D2"));
        }

        public FileEntity GetFile(Guid fileId)
        {
            return _fileRepository.GetById(fileId) ?? throw new FileNotFoundException("File not found with id " + fileId);
        }

        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            var dbFile = GetFile(fileId);
            return await _fileRetrievalService.RetrieveFileResource(dbFile);
        }

        public async Task<IActionResult> ViewImageById(Guid fileId, string DBFile, string dimensions, string scale)
        {
            var dbFile = GetFile(fileId);
            return await _fileRetrievalService.RetrieveImageResource(DBFile, dimensions, scale);
        }

        public async Task<IActionResult> ViewImageByUniqueKey(string uniqueKey,string DBFile ,string dimensions, string scale)
        {
            var dbFile = await _fileRepository.FindByUniqueKeyAsync(uniqueKey) ?? throw new FileNotFoundException("File not found");
            return await _fileRetrievalService.RetrieveImageResource(DBFile, dimensions, scale);
        }

        public async Task<bool> Delete(Guid fileId)
        {
            var fileEntity = GetFile(fileId);
            fileEntity.Deleted = true;
            fileEntity.DeletedAt = DateTime.UtcNow;
            await _fileRepository.UpdateAsync(fileEntity);
            return true;
        }

        public async Task<IEnumerable<FileDto>> SaveMultipleFiles(IEnumerable<IFormFile> files, SaveType type, string tenantId, string module)
        {
            var fileDtoList = new List<FileDto>();
            foreach (var file in files)
            {
                var fileDto = await SaveFile(file, type, tenantId, module);
                fileDtoList.Add(fileDto);
            }
            return fileDtoList;
        }
    }
}


