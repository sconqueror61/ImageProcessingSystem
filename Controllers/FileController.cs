using DocumentVerificationSystemApi.Models;
using DocumentVerificationSystemApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentVerificationSystemApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class FileController : ControllerBase
	{
		private readonly FileService _fileService;

		public FileController(FileService fileService)
		{
			_fileService = fileService;
		}

		[HttpPost("upload")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
		{
			if (request == null || request.File == null)
			{
				return BadRequest(new { message = "Geçersiz istek" });
			}

			var result = await _fileService.UploadFileAsync(request);

			if (!result.Success)
			{
				return BadRequest(new { message = result.Message });
			}

			return Ok(result);
		}

		[HttpGet("{fileId}")]
		public async Task<IActionResult> GetFile(Guid fileId, [FromQuery] Guid tanetId)
		{
			var file = await _fileService.GetFileAsync(fileId, tanetId);

			if (file == null)
			{
				return NotFound(new { message = "Dosya bulunamadı" });
			}

			return Ok(new
			{
				id = file.Id,
				fileName = file.FileName,
				fileExtension = file.FileExtension,
				fileSize = file.FileSize,
				ocrCompleted = file.OcrCompleted,
				ocrText = file.OcrText,
				ocrProcessedDate = file.OcrProcessedDate,
				isValidDocument = file.IsValidDocument,
				analysisResult = file.AnalysisResult,
				createdDate = file.CreatedDate,
				updatedDate = file.UpdatedDate
			});
		}

		[HttpGet("tenant/{tanetId}")]
		public async Task<IActionResult> GetFilesByTenant(Guid tanetId)
		{
			var files = await _fileService.GetFilesByTenantAsync(tanetId);

			var fileList = files.Select(f => new
			{
				id = f.Id,
				fileName = f.FileName,
				fileExtension = f.FileExtension,
				fileSize = f.FileSize,
				ocrCompleted = f.OcrCompleted,
				isValidDocument = f.IsValidDocument,
				createdDate = f.CreatedDate,
				updatedDate = f.UpdatedDate
			}).ToList();

			return Ok(fileList);
		}

		[HttpGet("{fileId}/download")]
		public async Task<IActionResult> DownloadFile(Guid fileId, [FromQuery] Guid tanetId)
		{
			var file = await _fileService.GetFileAsync(fileId, tanetId);

			if (file == null || file.FileData == null)
			{
				return NotFound(new { message = "Dosya bulunamadı" });
			}

			return File(file.FileData, "application/octet-stream", file.FileName);
		}

		[HttpDelete("{fileId}")]
		public async Task<IActionResult> DeleteFile(Guid fileId, [FromQuery] Guid tanetId)
		{
			var result = await _fileService.DeleteFileAsync(fileId, tanetId);

			if (!result)
			{
				return NotFound(new { message = "Dosya bulunamadı" });
			}

			return Ok(new { message = "Dosya başarıyla silindi" });
		}
	}
}