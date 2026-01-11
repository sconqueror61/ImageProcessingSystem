namespace DocumentVerificationSystemApi.Entity;

public class UserEntity : BaseEntity
{
	public string Name { get; set; }
	public string Surname { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public ICollection<UserRoleEntity> UserRoles { get; set; }
}
