namespace DocumentVerificationSystemApi.Models;

public class TenantRegisterResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public TenantInfo Tenant { get; set; }
}

public class TenantInfo
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Address { get; set; }
}