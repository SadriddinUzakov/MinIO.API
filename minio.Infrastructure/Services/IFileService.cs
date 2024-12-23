using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using minio.Domain.DTOs;
using minio.Domain.entity;
using minio.Domain.Enumerations;

namespace minio.Infrastructure.Services
{
    public interface IFileService
    {
        Task<FileDto> SaveFile(IFormFile file, SaveType type, string tenantId, string module);

        string PrepareUploadPath(string uploadPath);

        FileEntity GetFile(Guid fileId);

        Task<IActionResult> DownloadFile(Guid fileId);

        Task<IActionResult> ViewImageById(Guid fileId,string DBFile,string dimensions, string scale);

        Task<IActionResult> ViewImageByUniqueKey(string fileId,string DBFile, string dimensions, string scale);
        Task<bool> Delete(Guid fileId);

        Task<IEnumerable<FileDto>> SaveMultipleFiles(IEnumerable<IFormFile> files, SaveType type, string tenantId, string module);
    }
}

