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

	public async Task<LoginResponse> LoginAsync(LoginRequest request)
	{
		try
		{

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

	private string GenerateJwtToken(UserEntity user)
	{
		var jwtSettings = _configuration.GetSection("JwtSettings");
		var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGenerationMin32Characters";
		var issuer = jwtSettings["Issuer"] ?? "DocumentVerificationSystemApi";
		var audience = jwtSettings["Audience"] ?? "DocumentVerificationSystemApiUsers";

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
	{
		new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
		new Claim(ClaimTypes.Email, user.Email),
		new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}"),
		new Claim("tenant_id", user.TanetId.ToString()),
		new Claim("tenant_name", user.Tanet?.Name ?? string.Empty),
		new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
		new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
	};

		if (user.UserRoles != null)
		{
			foreach (var userRole in user.UserRoles)
			{
				if (userRole.Role != null)
				{
					claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Type));
				}
			}
		}

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims, // Listeyi veriyoruz
			expires: DateTime.UtcNow.AddHours(24),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
	public async Task<UserRegisterResponse> RegisterAsync(UserRegisterRequest request)
	{
		using (var transaction = _context.Database.BeginTransaction())
		{
			try
			{

				if (string.IsNullOrWhiteSpace(request.Email)) return new UserRegisterResponse { Success = false, Message = "Email gereklidir" };
				if (string.IsNullOrWhiteSpace(request.Password)) return new UserRegisterResponse { Success = false, Message = "Şifre gereklidir" };

				var tenant = await _context.Tanets.FirstOrDefaultAsync(t => t.Id == request.TanetId);
				if (tenant == null) return new UserRegisterResponse { Success = false, Message = "Geçersiz tenant ID" };

				var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);
				if (existingUser != null) return new UserRegisterResponse { Success = false, Message = "Bu email adresi zaten kullanılıyor" };

				string targetRole = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;

				var roleEntity = await _context.Roles
					.FirstOrDefaultAsync(r => r.Type.ToLower() == targetRole.ToLower() && !r.Isdeleted);

				if (roleEntity == null)
				{
					return new UserRegisterResponse
					{
						Success = false,
						Message = $"Sistemde '{targetRole}' adında bir yetki türü bulunamadı."
					};
				}

				// 5. Kullanıcıyı Oluştur
				var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
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

				var userRole = new UserRoleEntity
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					RoleId = roleEntity.Id
				};

				_context.UserRoles.Add(userRole);
				await _context.SaveChangesAsync();

				transaction.Commit();

				return new UserRegisterResponse
				{
					Success = true,
					Message = $"Kullanıcı başarıyla '{roleEntity.Type}' yetkisiyle kaydedildi.",
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
				transaction.Rollback(); // Hata olursa yapılan her şeyi geri al
				return new UserRegisterResponse
				{
					Success = false,
					Message = $"Bir hata oluştu: {ex.Message}"
				};
			}
		}
	}
}