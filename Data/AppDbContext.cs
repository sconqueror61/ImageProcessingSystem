using DocumentVerificationSystemApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerificationSystemApi.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
		 : base(options) { }

		public DbSet<UserEntity> Users { get; set; }
		public DbSet<RoleEntity> Roles { get; set; }
		public DbSet<UserRoleEntity> UserRoles { get; set; }
		public DbSet<TanetEntity> Tanets { get; set; }
		public DbSet<UploadFilesEntity> UploadFiles { get; set; }

	}
}
