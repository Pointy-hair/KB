using KnowledgeBank.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBank.Persistence
{
	public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<ApplicationUser>().ToTable("Users");

			builder.Entity<ApplicationUser>().ToTable("Users");
			builder.Entity<IdentityRole>().ToTable("Roles");
			builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
			builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
			builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
			builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
			builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

			builder.Entity<UserArea>()
				.ToTable("UsersAreas")
				.HasKey(ua => new { ua.UserId, ua.AreaId });

			builder.Entity<UserArea>()
				.HasOne(area => area.User)
				.WithMany(user => user.UserAreas)
				.HasForeignKey(ua => ua.UserId);

			builder.Entity<UserArea>()
				.HasOne(ua => ua.Area)
				.WithMany(t => t.AreaUsers)
				.HasForeignKey(ua => ua.AreaId);
		}

		public DbSet<Area> Areas { get; set; }
		public DbSet<UserArea> UsersAreas { get; set; }
	}
}
