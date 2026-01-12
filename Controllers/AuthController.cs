using DocumentVerificationSystemApi.Models;
using DocumentVerificationSystemApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace DocumentVerificationSystemApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly AuthenticationService _authenticationService;

		public AuthController(AuthenticationService authenticationService)
		{
			_authenticationService = authenticationService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
			{
				return BadRequest(new { message = "Email ve şifre gereklidir" });
			}

			var result = await _authenticationService.LoginAsync(request);

			if (!result.Success)
			{
				return Unauthorized(new { message = result.Message });
			}

			return Ok(result);
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
		{
			if (request == null)
			{
				return BadRequest(new { message = "Geçersiz istek" });
			}

			var result = await _authenticationService.RegisterAsync(request);

			if (!result.Success)
			{
				return BadRequest(new { message = result.Message });
			}

			return Ok(result);
		}
	}
}