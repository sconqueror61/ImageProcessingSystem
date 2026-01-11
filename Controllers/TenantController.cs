using DocumentVerificationSystemApi.Models;
using DocumentVerificationSystemApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace DocumentVerificationSystemApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TenantController : ControllerBase
	{
		private readonly TenantService _tenantService;

		public TenantController(TenantService tenantService)
		{
			_tenantService = tenantService;
		}

		/// <summary>
		/// Yeni tenant kaydı oluşturur
		/// </summary>
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] TenantRegisterRequest request)
		{
			if (request == null)
			{
				return BadRequest(new { message = "Geçersiz istek" });
			}

			var result = await _tenantService.RegisterAsync(request);

			if (!result.Success)
			{
				return BadRequest(new { message = result.Message });
			}

			return Ok(result);
		}
	}
}