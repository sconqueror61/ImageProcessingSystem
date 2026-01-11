namespace DocumentVerificationSystemApi.Entity
{
	public class UserRoleEntity
	{

		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid RoleId { get; set; }
		public UserEntity User;
		public RoleEntity Role;
	}
}
