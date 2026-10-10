using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var users = modelBuilder.Entity<User>();
        users.ToTable("Users");
        users.HasKey(x => x.Id);
        users.Property(x => x.Id).ValueGeneratedNever();
        users.Property(x => x.Name).HasMaxLength(100).IsRequired();
        users.Property(x => x.Email).HasMaxLength(254).IsRequired();
        users.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        users.HasIndex(x => x.Email).IsUnique();
        users.HasMany(x => x.Roles).WithMany().UsingEntity<Dictionary<string, object>>(
            "UserRoles",
            right => right.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
            left => left.HasOne<User>().WithMany().HasForeignKey("UserId"),
            join => join.HasKey("UserId", "RoleId"));
        users.Navigation(x => x.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);

        var roles = modelBuilder.Entity<Role>();
        roles.ToTable("Roles");
        roles.HasKey(x => x.Id);
        roles.Property(x => x.Id).ValueGeneratedNever();
        roles.Property(x => x.Name).HasMaxLength(100).IsRequired();
        roles.HasIndex(x => x.Name).IsUnique();
        roles.HasMany(x => x.Permissions).WithMany().UsingEntity<Dictionary<string, object>>(
            "RolePermissions",
            right => right.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
            left => left.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
            join => join.HasKey("RoleId", "PermissionId"));
        roles.Navigation(x => x.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);

        var permissions = modelBuilder.Entity<Permission>();
        permissions.ToTable("Permissions");
        permissions.HasKey(x => x.Id);
        permissions.Property(x => x.Id).ValueGeneratedNever();
        permissions.Property(x => x.Name).HasMaxLength(100).IsRequired();
        permissions.HasIndex(x => x.Name).IsUnique();
    }
}
