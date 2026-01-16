using DocumentVerificationSystemApi.Models;
using DocumentVerificationSystemApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace DocumentVerificationSystemApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OcrController : ControllerBase
	{
		private readonly OcrService _ocrService;
		private readonly GeminiServices _geminiServices;

		public OcrController(OcrService ocrService, GeminiServices geminiServices)
		{
			_ocrService = ocrService;
			this._geminiServices = geminiServices;
		}


		[HttpPost("ImageAnalyze")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> ReadImage([FromForm] OcrImageRequest request)
		{
			if (request.File == null || request.File.Length == 0)
				return BadRequest("Dosya gönderilmedi");

			// Dosya formatını kontrol et (PDF desteklenmiyor - sadece görüntü formatları)
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff" };
			var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
			if (!allowedExtensions.Contains(fileExtension))
			{
				if (fileExtension == ".pdf")
					return BadRequest("PDF dosyaları desteklenmiyor. Lütfen PDF'yi görüntü formatına (JPG, PNG) dönüştürün.");
				return BadRequest($"Desteklenmeyen dosya formatı. İzin verilen formatlar: {string.Join(", ", allowedExtensions)}");
			}

			using var memoryStream = new MemoryStream();
			await request.File.CopyToAsync(memoryStream);
			var imageBytes = memoryStream.ToArray();

			// OCR işlemi yap
			var text = await _ocrService.ReadTextAsync(imageBytes);
			string prompt = text + _geminiServices.OlusturVergiLevhasiPromptu;
			var result = await _geminiServices.SoruSorAsync(prompt);
			return Ok(new { result, text });
		}
	}
}