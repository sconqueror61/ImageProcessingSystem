using DocumentVerificationSystemApi.Data;
using DocumentVerificationSystemApi.Entity;
using DocumentVerificationSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerificationSystemApi.Service;

public class TenantService
{
	private readonly AppDbContext _context;

	public TenantService(AppDbContext context)
	{
		_context = context;
	}

	/// <summary>
	/// Yeni tenant kaydı oluşturur
	/// </summary>
	public async Task<TenantRegisterResponse> RegisterAsync(TenantRegisterRequest request)
	{
		try
		{
			// Validasyon
			if (string.IsNullOrWhiteSpace(request.Name))
			{
				return new TenantRegisterResponse
				{
					Success = false,
					Message = "Tenant adı gereklidir"
				};
			}

			if (string.IsNullOrWhiteSpace(request.Address))
			{
				return new TenantRegisterResponse
				{
					Success = false,
					Message = "Adres gereklidir"
				};
			}

			// Aynı isimde tenant var mı kontrol et
			var existingTenant = await _context.Tanets
				.FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower());

			if (existingTenant != null)
			{
				return new TenantRegisterResponse
				{
					Success = false,
					Message = "Bu isimde bir tenant zaten mevcut"
				};
			}

			// Yeni tenant oluştur
			var tenant = new TanetEntity
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				Adress = request.Address
			};

			_context.Tanets.Add(tenant);
			await _context.SaveChangesAsync();

			return new TenantRegisterResponse
			{
				Success = true,
				Message = "Tenant başarıyla kaydedildi",
				Tenant = new TenantInfo
				{
					Id = tenant.Id,
					Name = tenant.Name,
					Address = tenant.Adress
				}
			};
		}
		catch (Exception ex)
		{
			return new TenantRegisterResponse
			{
				Success = false,
				Message = $"Bir hata oluştu: {ex.Message}"
			};
		}
	}

	/// <summary>
	/// Tenant ID ile tenant'ı getirir
	/// </summary>
	public async Task<TanetEntity> GetByIdAsync(Guid tenantId)
	{
		return await _context.Tanets.FirstOrDefaultAsync(t => t.Id == tenantId);
	}
}