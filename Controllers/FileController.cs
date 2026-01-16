using DocumentVerificationSystemApi.Models;
using DocumentVerificationSystemApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

		[HttpPost("AnalyzeFile")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> AnalyzeFile([FromForm] FileUploadRequest request)
		{
			if (request == null || request.File == null)
			{
				return BadRequest(new { success = false, message = "Geçersiz istek: Dosya yok." });
			}

			var result = await _fileService.AnalyzeFileAsync(request);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		// 2. KAYDET (Angular: Sarı Buton)
		[HttpPost("SaveDetails")]
		public async Task<IActionResult> SaveDetails([FromBody] SaveDetailsRequest request)
		{
			if (request == null)
			{
				return BadRequest(new { success = false, message = "İstek boş olamaz." });
			}

			var result = await _fileService.SaveDetailsAsync(request);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		[HttpGet("dashboard-details")]
		public async Task<IActionResult> GetDashboardDetails([FromQuery] Guid tanetId, [FromQuery] int page = 1)
		{

			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
			Guid userId = Guid.Parse(userIdClaim);

			var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
			string userRole = roleClaim ?? "User";

			var result = await _fileService.GetUserDetailsPagedAsync(userId, userRole, tanetId, page);

			return Ok(result);
		}

		[HttpDelete("{fileId}")]
		public async Task<IActionResult> DeleteFile(Guid fileId, [FromQuery] Guid tanetId)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

			if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

			Guid userId = Guid.Parse(userIdClaim);
			string userRole = roleClaim ?? "User";

			var result = await _fileService.DeleteFileAsync(fileId, userId, userRole, tanetId);

			if (!result.Success)
				return BadRequest(result);

			return Ok(result);
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
	}
}