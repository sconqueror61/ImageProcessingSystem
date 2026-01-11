using DocumentVerificationSystemApi.Data;
using DocumentVerificationSystemApi.Entity;
using DocumentVerificationSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocumentVerificationSystemApi.Service;

public class AuthenticationService
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;

	public AuthenticationService(AppDbContext context, IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}

	/// <summary>
	/// Kullanıcı giriş işlemini gerçekleştirir
	/// </summary>
	public async Task<LoginResponse> LoginAsync(LoginRequest request)
	{
		try
		{
			// Email ile kullanıcıyı bul (Tanet bilgisini de yükle)
			var user = await _context.Users
				.Include(u => u.Tanet)
				.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

			if (user == null)
			{
				return new LoginResponse
				{
					Success = false,
					Message = "Email veya şifre hatalı"
				};
			}

			// Şifre kontrolü
			if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
			{
				return new LoginResponse
				{
					Success = false,
					Message = "Email veya şifre hatalı"
				};
			}

			// JWT token oluştur
			var token = GenerateJwtToken(user);

			return new LoginResponse
			{
				Success = true,
				Token = token,
				Message = "Giriş başarılı",
				User = new UserInfo
				{
					Id = user.Id,
					Name = user.Name,
					Surname = user.Surname,
					Email = user.Email
				}
			};
		}
		catch (Exception ex)
		{
			return new LoginResponse
			{
				Success = false,
				Message = $"Bir hata oluştu: {ex.Message}"
			};
		}
	}

	/// <summary>
	/// JWT token oluşturur
	/// </summary>
	private string GenerateJwtToken(UserEntity user)
	{
		var jwtSettings = _configuration.GetSection("JwtSettings");
		var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGenerationMin32Characters";
		var issuer = jwtSettings["Issuer"] ?? "DocumentVerificationSystemApi";
		var audience = jwtSettings["Audience"] ?? "DocumentVerificationSystemApiUsers";

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}"),
			new Claim("tenant_id", user.TanetId.ToString()),
			new Claim("tenant_name", user.Tanet?.Name ?? string.Empty),
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			expires: DateTime.UtcNow.AddHours(24),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	/// <summary>
	/// Yeni kullanıcı kaydı oluşturur
	/// </summary>
	public async Task<UserRegisterResponse> RegisterAsync(UserRegisterRequest request)
	{
		try
		{
			// Validasyon
			if (string.IsNullOrWhiteSpace(request.Email))
			{
				return new UserRegisterResponse
				{
					Success = false,
					Message = "Email gereklidir"
				};
			}

			if (string.IsNullOrWhiteSpace(request.Password))
			{
				return new UserRegisterResponse
				{
					Success = false,
					Message = "Şifre gereklidir"
				};
			}

			if (string.IsNullOrWhiteSpace(request.Name))
			{
				return new UserRegisterResponse
				{
					Success = false,
					Message = "Ad gereklidir"
				};
			}

			if (string.IsNullOrWhiteSpace(request.Surname))
			{
				return new UserRegisterResponse
				{
					Success = false,
					Message = "Soyad gereklidir"
				};
			}

			// Tenant kontrolü
			var tenant = await _context.Tanets.FirstOrDefaultAsync(t => t.Id == request.TanetId);
			if (tenant == null)
			{
				return new UserRegisterResponse
				{
					Success = false,
					Message = "Geçersiz tenant ID"
				};
			}

			// Email kontrolü
			var existingUser = await _context.Users
				.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);

			if (existingUser != null)
			{
				return new UserRegisterResponse
				{
					Success = false,
					Message = "Bu email adresi zaten kullanılıyor"
				};
			}

			// Şifreyi hashle
			var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

			// Yeni kullanıcı oluştur
			var user = new UserEntity
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				Surname = request.Surname,
				Email = request.Email,
				Password = hashedPassword,
				TanetId = request.TanetId,
				IsDeleted = false,
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			return new UserRegisterResponse
			{
				Success = true,
				Message = "Kullanıcı başarıyla kaydedildi",
				User = new UserInfo
				{
					Id = user.Id,
					Name = user.Name,
					Surname = user.Surname,
					Email = user.Email
				}
			};
		}
		catch (Exception ex)
		{
			return new UserRegisterResponse
			{
				Success = false,
				Message = $"Bir hata oluştu: {ex.Message}"
			};
		}
	}
}