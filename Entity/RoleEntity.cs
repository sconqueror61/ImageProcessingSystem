using DocumentVerificationSystemApi.Entity;

public class RoleEntity
{
	public Guid Id { get; set; }
	public string Type { get; set; }
	public bool Isdeleted { get; set; }

	public ICollection<UserRoleEntity> UserRoles { get; set; }
}