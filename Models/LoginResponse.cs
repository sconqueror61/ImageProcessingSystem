namespace DocumentVerificationSystemApi.Models;

public class LoginResponse
{
	public bool Success { get; set; }
	public string Token { get; set; }
	public string Message { get; set; }
	public UserInfo User { get; set; }
}

public class UserInfo
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
	public string Email { get; set; }
}