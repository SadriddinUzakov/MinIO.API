using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using minio.Application.Core.Exceptions;
using minio.Domain.DTOs;
using minio.Domain.entity;
using minio.Domain.Enumerations;
using minio.Infrastructure.Repositories;
using FileNotFoundException = minio.Application.Core.Exceptions.FileNotFoundException;

namespace minio.Infrastructure.Services.file
{
    public class FileServiceImpl
    {
        private readonly IFileRepository _fileRepository;
        private readonly FileRetrievalService _fileRetrievalService;
        private readonly ILogger<FileServiceImpl> _logger;
        private readonly string _imagesRootPath = "uploads/images/";
        private readonly string _filesRootPath = "uploads/files/";
        private readonly string _defaultUploadPath = "{YYYY}/{MM}/{DD}/";

        public FileServiceImpl(IFileRepository fileRepository, FileRetrievalService fileRetrievalService, ILogger<FileServiceImpl> logger)
        {
            _fileRepository = fileRepository;
            _fileRetrievalService = fileRetrievalService;
            _logger = logger;
        }

        public async Task<FileDto> SaveFile(IFormFile file, SaveType type, string tenantId, string module)
        {
            var rootPath = _filesRootPath;
            var uploadPath = new StringBuilder(type == SaveType.PRIVATE ? SaveType.PRIVATE.ToString() : SaveType.PUBLIC.ToString());
            var realUploadPath = PrepareUploadPath(uploadPath.Append("/").Append(tenantId).Append("/").Append(module.ToUpper()).Append("/").Append(_defaultUploadPath).ToString());
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);

            if (realUploadPath.Contains(".."))
                throw new FileStorageException("Filename contains invalid path sequence " + fileName);

            if (fileExtension != null && new[] { "JPG", "JPEG", "PNG" }.Contains(fileExtension.ToUpper()))
                rootPath = _imagesRootPath;

            try
            {
                var fileEntity = new FileEntity
                {
                    Name = fileName,
                    Extension = fileExtension,
                    Size = file.Length,
                    ContentType = file.ContentType,
                    CreatedAt = DateTime.UtcNow,
                    SaveMode = SaveMode.SYSTEM_SERVER
                };
                await _fileRepository.AddAsync(fileEntity);

                var filePath = Path.Combine(rootPath, realUploadPath, fileEntity.Id + fileEntity.Extension);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                fileEntity.Path = filePath;
                await _fileRepository.UpdateAsync(fileEntity);

                return new FileDto
                {
                    Id = fileEntity.Id,
                    Name = fileEntity.Name,
                    Size = fileEntity.Size,
                    Extension = fileEntity.Extension,
                    ContentType = fileEntity.ContentType,
                    CreatedAt = fileEntity.CreatedAt.ToString("o")
                };
            }
            catch (Exception e)
            {
                var msg = $"Could not store file {fileName}. Please try again!";
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

        public async Task<IActionResult> ViewImageById(Guid fileId,string dbFile, string dimensions, string scale)
        {
            var dbFilee = GetFile(fileId);
            return await _fileRetrievalService.RetrieveImageResource(dbFile, dimensions, scale);
        }

        public async Task<IActionResult> ViewImageByUniqueKey(string fileId, string dimensions, string scale)
        {
            // TODO: Implement this method if needed
            return null;
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

