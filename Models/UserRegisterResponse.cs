namespace DocumentVerificationSystemApi.Models;

public class UserRegisterResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public UserInfo User { get; set; }
}