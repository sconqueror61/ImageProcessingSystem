using DocumentVerificationSystemApi.Data;
using DocumentVerificationSystemApi.Entity;
using DocumentVerificationSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerificationSystemApi.Service;

public class FileService
{
	private readonly AppDbContext _context;
	private readonly OcrService _ocrService;
	private readonly GeminiServices _geminiServices;

	public FileService(AppDbContext context, OcrService ocrService, GeminiServices geminiServices)
	{
		_context = context;
		_ocrService = ocrService;
		_geminiServices = geminiServices;
	}

	/// <summary>
	/// Dosya yükler ve OCR işlemi yapar
	/// </summary>
	public async Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request)
	{
		try
		{
			// Validasyon
			if (request.File == null || request.File.Length == 0)
			{
				return new FileUploadResponse
				{
					Success = false,
					Message = "Dosya gönderilmedi"
				};
			}

			// Tenant kontrolü
			var tenant = await _context.Tanets.FirstOrDefaultAsync(t => t.Id == request.TanetId);
			if (tenant == null)
			{
				return new FileUploadResponse
				{
					Success = false,
					Message = "Geçersiz tenant ID"
				};
			}

			// Dosya formatını kontrol et
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".pdf" };
			var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
			if (!allowedExtensions.Contains(fileExtension))
			{
				return new FileUploadResponse
				{
					Success = false,
					Message = $"Desteklenmeyen dosya formatı. İzin verilen formatlar: {string.Join(", ", allowedExtensions)}"
				};
			}

			// Dosya boyutu kontrolü (100 MB limit)
			const long maxFileSize = 100 * 1024 * 1024; // 100 MB
			if (request.File.Length > maxFileSize)
			{
				return new FileUploadResponse
				{
					Success = false,
					Message = "Dosya boyutu 100 MB'dan büyük olamaz"
				};
			}


			// OCR işlemi yap


			// Dosya içeriğini oku
			byte[] fileData;
			using (var memoryStream = new MemoryStream())
			{
				await request.File.CopyToAsync(memoryStream);
				fileData = memoryStream.ToArray();
			}

			var text = await _ocrService.ReadTextAsync(fileData);
			var result = await _geminiServices.SoruSorAsync(text + "bu metni satır satır parçalayarak anlamlı bir yapıya getir.");

			// Yeni dosya kaydı oluştur
			var uploadFile = new UploadFilesEntity
			{
				Id = Guid.NewGuid(),
				OcrText = text,
				OcrProcessedDate = DateTime.UtcNow,
				AnalysisResult = result,
				FileName = request.File.FileName,
				FileExtension = fileExtension,
				FileSize = request.File.Length,
				FileData = fileData,
				TanetId = request.TanetId,
				OcrCompleted = false,
				IsValidDocument = false,
				IsDeleted = false,
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow
			};

			_context.UploadFiles.Add(uploadFile);
			await _context.SaveChangesAsync();

			// OCR işlemini arka planda başlat (async)
			_ = Task.Run(async () => await ProcessOcrAsync(uploadFile.Id));

			return new FileUploadResponse
			{
				Success = true,
				Message = "Dosya başarıyla yüklendi. OCR işlemi başlatıldı.",
				File = new DocumentVerificationSystemApi.Models.FileInfo
				{
					Id = uploadFile.Id,
					FileName = uploadFile.FileName,
					FileExtension = uploadFile.FileExtension,
					FileSize = uploadFile.FileSize,
					OcrCompleted = uploadFile.OcrCompleted,
					IsValidDocument = uploadFile.IsValidDocument,
					CreatedDate = uploadFile.CreatedDate
				}
			};
		}
		catch (Exception ex)
		{
			return new FileUploadResponse
			{
				Success = false,
				Message = $"Bir hata oluştu: {ex.Message}"
			};
		}
	}

	/// <summary>
	/// OCR işlemini yapar ve sonuçları kaydeder
	/// </summary>
	private async Task ProcessOcrAsync(Guid fileId)
	{
		try
		{
			var uploadFile = await _context.UploadFiles.FirstOrDefaultAsync(f => f.Id == fileId);
			if (uploadFile == null) return;

			// OCR işlemi (sadece görüntü formatları için)
			var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff" };
			if (imageExtensions.Contains(uploadFile.FileExtension.ToLower()))
			{
				var ocrText = await _ocrService.ReadTextAsync(uploadFile.FileData);
				uploadFile.OcrText = ocrText;
				uploadFile.OcrCompleted = true;
				uploadFile.OcrProcessedDate = DateTime.UtcNow;

				// Gemini ile analiz yap
				if (!string.IsNullOrWhiteSpace(ocrText))
				{
					var analysisPrompt = $"{ocrText} Bu metni analiz ederek belgenin geçerli olup olmadığını ve sonuçları JSON formatında döndür.";
					var analysisResult = await _geminiServices.SoruSorAsync(analysisPrompt);
					uploadFile.AnalysisResult = analysisResult;

					// Basit validasyon kontrolü (analiz sonucuna göre)
					uploadFile.IsValidDocument = !string.IsNullOrWhiteSpace(ocrText) && ocrText.Length > 10;
				}

				uploadFile.UpdatedDate = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
			else
			{
				// PDF veya diğer formatlar için OCR yapılamıyor
				uploadFile.OcrCompleted = true;
				uploadFile.OcrProcessedDate = DateTime.UtcNow;
				uploadFile.OcrText = "Bu dosya formatı için OCR işlemi desteklenmiyor.";
				await _context.SaveChangesAsync();
			}
		}
		catch (Exception ex)
		{
			// Hata durumunda kaydı güncelle
			var uploadFile = await _context.UploadFiles.FirstOrDefaultAsync(f => f.Id == fileId);
			if (uploadFile != null)
			{
				uploadFile.OcrCompleted = true;
				uploadFile.OcrText = $"OCR işlemi sırasında hata oluştu: {ex.Message}";
				uploadFile.UpdatedDate = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
		}
	}

	/// <summary>
	/// Dosya bilgilerini getirir
	/// </summary>
	public async Task<UploadFilesEntity> GetFileAsync(Guid fileId, Guid tanetId)
	{
		return await _context.UploadFiles
			.Include(f => f.Tanet)
			.FirstOrDefaultAsync(f => f.Id == fileId && f.TanetId == tanetId && !f.IsDeleted);
	}

	/// <summary>
	/// Tenant'a ait tüm dosyaları getirir
	/// </summary>
	public async Task<List<UploadFilesEntity>> GetFilesByTenantAsync(Guid tanetId)
	{
		return await _context.UploadFiles
			.Where(f => f.TanetId == tanetId && !f.IsDeleted)
			.OrderByDescending(f => f.CreatedDate)
			.ToListAsync();
	}

	/// <summary>
	/// Dosya siler (soft delete)
	/// </summary>
	public async Task<bool> DeleteFileAsync(Guid fileId, Guid tanetId)
	{
		var uploadFile = await _context.UploadFiles
			.FirstOrDefaultAsync(f => f.Id == fileId && f.TanetId == tanetId && !f.IsDeleted);

		if (uploadFile == null)
			return false;

		uploadFile.IsDeleted = true;
		uploadFile.UpdatedDate = DateTime.UtcNow;
		await _context.SaveChangesAsync();

		return true;
	}
}