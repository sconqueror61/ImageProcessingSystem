public class UserRegisterRequest
{
	public string Name { get; set; }
	public string Surname { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public Guid TanetId { get; set; }
	public string Role { get; set; } // <--- Bak burada "Role" yazıyor, "User" değil.
}