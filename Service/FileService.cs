using DocumentVerificationSystemApi.Data;
using DocumentVerificationSystemApi.Entity;
using DocumentVerificationSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DocumentVerificationSystemApi.Service
{
	public class FileService
	{
		private readonly AppDbContext _context;
		private readonly OcrService _ocrService;
		private readonly GeminiServices _geminiServices;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public FileService(AppDbContext context, OcrService ocrService, GeminiServices geminiServices, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_ocrService = ocrService;
			_geminiServices = geminiServices;
			_httpContextAccessor = httpContextAccessor;
		}

		// --- 1. ADIM: DOSYAYI YÜKLE VE ANALİZ ET (Veritabanına Dosya Olarak Kaydet) ---
		public async Task<FileUploadResponse> AnalyzeFileAsync(FileUploadRequest request)
		{
			// Validasyonlar
			if (request.File == null || request.File.Length == 0)
				return new FileUploadResponse { Success = false, Message = "Dosya yok" };

			var tenant = await _context.Tanets.FirstOrDefaultAsync(t => t.Id == request.TanetId);
			if (tenant == null)
				return new FileUploadResponse { Success = false, Message = "Geçersiz Tenant" };

			try
			{
				// Dosyayı Byte'a çevir
				byte[] fileData;
				using (var memoryStream = new MemoryStream())
				{
					await request.File.CopyToAsync(memoryStream);
					fileData = memoryStream.ToArray();
				}

				// OCR İşlemi
				var ocrText = await _ocrService.ReadTextAsync(fileData);

				// Gemini AI İşlemi
				string prompt = _geminiServices.OlusturVergiLevhasiPromptu(ocrText);
				string geminiResult = await _geminiServices.SoruSorAsync(prompt);

				if (string.IsNullOrEmpty(geminiResult))
					return new FileUploadResponse { Success = false, Message = "Yapay zeka yanıt vermedi." };

				// Dosyayı UploadFiles tablosuna kaydet
				var userId = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
				var newFileId = Guid.NewGuid();
				var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

				var uploadFile = new UploadFilesEntity
				{
					Id = newFileId,
					CreaterId = userId,
					FileName = request.File.FileName,
					FileExtension = fileExtension,
					FileSize = request.File.Length,
					FileData = fileData,
					TanetId = request.TanetId,
					OcrText = ocrText,
					AnalysisResult = geminiResult,
					CreatedDate = DateTime.UtcNow,
					IsDeleted = false
				};

				_context.UploadFiles.Add(uploadFile);
				await _context.SaveChangesAsync();

				// Geriye dosya ID'si ve analiz sonucunu dön
				return new FileUploadResponse
				{
					Success = true,
					Message = "Analiz tamamlandı. Kaydetmek için butonu kullanın.",
					File = new DocumentVerificationSystemApi.Models.FileInfo
					{
						Id = uploadFile.Id,
						FileName = uploadFile.FileName,
						AnalysisResult = geminiResult
					}
				};
			}
			catch (Exception ex)
			{
				return new FileUploadResponse { Success = false, Message = "Hata: " + ex.Message };
			}
		}

		// --- 2. ADIM: SAVE BUTONUNA BASINCA DETAYLARI KAYDET ---
		public async Task<FileUploadResponse> SaveDetailsAsync(SaveDetailsRequest request)
		{
			if (string.IsNullOrEmpty(request.AnalysisResult))
				return new FileUploadResponse { Success = false, Message = "Analiz sonucu boş olamaz." };

			// Dosyayı kontrol et
			var existingFile = await _context.UploadFiles.FirstOrDefaultAsync(f => f.Id == request.FileId);
			if (existingFile == null)
				return new FileUploadResponse { Success = false, Message = "Dosya bulunamadı." };

			try
			{
				// JSON Temizleme
				string cleanJson = request.AnalysisResult.Replace("```json", "").Replace("```", "").Trim();
				var details = JsonConvert.DeserializeObject<DetailsEntity>(cleanJson);

				if (details != null)
				{
					details.Id = Guid.NewGuid();
					details.UploadFileId = request.FileId;
					details.TanetId = request.TanetId;
					details.CreatedDate = DateTime.UtcNow;
					details.IsDeleted = false;

					_context.Details.Add(details);
					await _context.SaveChangesAsync();

					return new FileUploadResponse { Success = true, Message = "Veriler veritabanına başarıyla işlendi." };
				}

				return new FileUploadResponse { Success = false, Message = "JSON verisi uygun formatta değil." };
			}
			catch (Exception ex)
			{
				return new FileUploadResponse { Success = false, Message = "JSON Parse Hatası: " + ex.Message };
			}
		}
		public async Task<PagedResult<DashboardDetailUser>> GetUserDetailsPagedAsync(Guid userId, string userRole, Guid tanetId, int pageNumber, int pageSize = 10)
		{
			// Details ve UploadFiles tablolarını birleştiriyoruz
			var query = from d in _context.Details
						join u in _context.UploadFiles on d.UploadFileId equals u.Id
						where u.TanetId == tanetId && !d.IsDeleted && !u.IsDeleted
						select new DashboardDetailUser
						{
							// ID Bilgileri
							DetailId = d.Id,
							FileId = u.Id,
							CreaterId = u.CreaterId, // Admin kontrolü için şart

							// Details Tablosundan Gelen Tüm Veriler
							DocumentType = d.DocumentType,
							TaxpayerType = d.TaxpayerType,
							CompanyType = d.CompanyType,
							CompanyName = d.CompanyName,
							TaxNumber = d.TaxNumber,
							TcIdentityNumber = d.TcIdentityNumber,
							TaxOffice = d.TaxOffice,
							ActivityCode = d.ActivityCode,
							GrossIncome = d.GrossIncome,
							TaxBase = d.TaxBase,
							CalculatedTax = d.CalculatedTax,
							AccruedTax = d.AccruedTax,
							TaxPeriod = d.TaxPeriod,
							ExtractedConfidence = d.ExtractedConfidence,
							CreatedDate = d.CreatedDate,

							// File Tablosundan Gelen Veriler
							FileName = u.FileName,
							FileExtension = u.FileExtension
						};

			// ROL KONTROLÜ: Admin değilse sadece kendi yüklediklerini görsün
			if (userRole.ToLower() != "admin")
			{
				query = query.Where(x => x.CreaterId == userId);
			}

			// Sayfalama İşlemleri
			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(x => x.CreatedDate)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<DashboardDetailUser>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize
			};
		}

		// --- 4. SİLME METODU ---
		public async Task<FileUploadResponse> DeleteFileAsync(Guid fileId, Guid userId, string userRole, Guid tanetId)
		{
			var uploadFile = await _context.UploadFiles
				.FirstOrDefaultAsync(f => f.Id == fileId && f.TanetId == tanetId && !f.IsDeleted);

			if (uploadFile == null)
				return new FileUploadResponse { Success = false, Message = "Dosya bulunamadı." };

			if (userRole.ToLower() != "admin" && uploadFile.CreaterId != userId)
			{
				return new FileUploadResponse { Success = false, Message = "Bu dosyayı silme yetkiniz yok." };
			}

			uploadFile.IsDeleted = true;
			uploadFile.UpdatedDate = DateTime.UtcNow;

			var details = await _context.Details.Where(d => d.UploadFileId == fileId).ToListAsync();
			foreach (var det in details)
			{
				det.IsDeleted = true;
			}

			await _context.SaveChangesAsync();
			return new FileUploadResponse { Success = true, Message = "Dosya başarıyla silindi." };
		}

		public async Task<UploadFilesEntity?> GetFileAsync(Guid fileId, Guid tanetId)
		{
			return await _context.UploadFiles
				.FirstOrDefaultAsync(f => f.Id == fileId && f.TanetId == tanetId && !f.IsDeleted);
		}

		public async Task<List<UploadFilesEntity>> GetFilesByTenantAsync(Guid tanetId)
		{
			return await _context.UploadFiles
				.Where(f => f.TanetId == tanetId && !f.IsDeleted)
				.OrderByDescending(f => f.CreatedDate)
				.ToListAsync();
		}
	}
}