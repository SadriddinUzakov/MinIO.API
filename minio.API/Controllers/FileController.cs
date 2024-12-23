using Microsoft.AspNetCore.Mvc;
using minio.Domain.DTOs;
using minio.Domain.Enumerations;
using minio.Infrastructure.Services;
namespace minio.API.controller
{
    [ApiController]
    [Route("api/file")]
    public class FileController
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("upload/public")]
        [Consumes("multipart/form-data")]
        public async Task<FileDto> UploadPublic(IFormFile file, [FromQuery] string tenantId, [FromQuery] string module)
        {
            return await _fileService.SaveFile(file, SaveType.PUBLIC, tenantId, module);
        }

        [HttpPost("upload/private")]
        [Consumes("multipart/form-data")]
        public async Task<FileDto> UploadPrivate(IFormFile file, [FromQuery] string tenantId, [FromQuery] string module)
        {
            return await _fileService.SaveFile(file, SaveType.PRIVATE, tenantId, module);
        }

        [HttpPost("multiple-upload/public")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IEnumerable<FileDto>> UploadMultipleFilesAsPublic(IFormFile[] files, [FromQuery] string tenantId, [FromQuery] string module)
        {
            return await _fileService.SaveMultipleFiles(files.ToList(), SaveType.PUBLIC, tenantId, module);
        }

        [HttpPost("multiple-upload/private")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IEnumerable<FileDto>> UploadMultipleFilesAsPrivate(IFormFile[] files, [FromQuery] string tenantId, [FromQuery] string module)
        {
            return await _fileService.SaveMultipleFiles(files.ToList(), SaveType.PRIVATE, tenantId, module);
        }

        [HttpGet("download-file/private/{fileId}")]
        public async Task<IActionResult> DownloadPrivateFile(Guid fileId)
        {
            var resource = await _fileService.DownloadFile(fileId);
            //return File(resource, "application/octet-stream");
            return resource;
        }

        [HttpGet("download-file/public/{fileId}")]
        public async Task<IActionResult> DownloadPublicFile(Guid fileId)
        {
            var resource = await _fileService.DownloadFile(fileId);
            //return File(resource, "application/octet-stream");
            return resource;
        }

        [HttpGet("view-image/public/{uniqueKey}")]
        public async Task<IActionResult> ViewPublicImageByUniqueKey(string uniqueKey, [FromQuery] string DBFile = null,[FromQuery] string dimensions = null, [FromQuery] string scale = null)
        {
            var resource = await _fileService.ViewImageByUniqueKey(uniqueKey, DBFile, dimensions, scale);
            //return File(resource, "image/jpeg");
            return resource;
        }

        [HttpGet("view-image/private/{uniqueKey}")]
        public async Task<IActionResult> ViewPrivateImageByObjectName(string uniqueKey, [FromQuery]string DBFile = null,[FromQuery] string dimensions = null, [FromQuery] string scale = null)
        {
            var resource = await _fileService.ViewImageByUniqueKey(uniqueKey,DBFile,dimensions, scale);
            //return File(resource, "image/jpeg");
            return resource;
        }

        [HttpDelete("delete/{fileId}")]
        public async Task<IActionResult> Delete(Guid fileId)
        {
            var result = await _fileService.Delete(fileId);
            return Ok(result);
        }

        private IActionResult Ok(bool result)
        {
            throw new NotImplementedException();
        }
    }
}
