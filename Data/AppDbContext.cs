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
		public DbSet<DetailsEntity> Details { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			modelBuilder.Entity<UserRoleEntity>()
				.HasOne(ur => ur.User)
				.WithMany(u => u.UserRoles)
				.HasForeignKey(ur => ur.UserId);

			modelBuilder.Entity<UserRoleEntity>()
				.HasOne(ur => ur.Role)
				.WithMany(r => r.UserRoles)
				.HasForeignKey(ur => ur.RoleId);

			base.OnModelCreating(modelBuilder);
		}
	}
}
