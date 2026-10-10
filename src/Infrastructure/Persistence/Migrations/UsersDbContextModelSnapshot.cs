using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(UsersDbContext))]
public partial class UsersDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.21")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);
        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("Domain.Permission", b =>
        {
            b.Property<int>("Id").HasColumnType("int");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.HasKey("Id");
            b.HasIndex("Name").IsUnique();
            b.ToTable("Permissions");
        });

        modelBuilder.Entity("Domain.Role", b =>
        {
            b.Property<int>("Id").HasColumnType("int");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.HasKey("Id");
            b.HasIndex("Name").IsUnique();
            b.ToTable("Roles");
        });

        modelBuilder.Entity("Domain.User", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uniqueidentifier");
            b.Property<string>("Email").IsRequired().HasMaxLength(254).HasColumnType("nvarchar(254)");
            b.Property<bool>("IsActive").HasColumnType("bit");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("PasswordHash").IsRequired().HasMaxLength(512).HasColumnType("nvarchar(512)");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Users");
        });

        modelBuilder.Entity("RolePermissions", b =>
        {
            b.Property<int>("RoleId").HasColumnType("int");
            b.Property<int>("PermissionId").HasColumnType("int");
            b.HasKey("RoleId", "PermissionId");
            b.HasIndex("PermissionId");
            b.ToTable("RolePermissions");
        });

        modelBuilder.Entity("UserRoles", b =>
        {
            b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
            b.Property<int>("RoleId").HasColumnType("int");
            b.HasKey("UserId", "RoleId");
            b.HasIndex("RoleId");
            b.ToTable("UserRoles");
        });

        modelBuilder.Entity("RolePermissions", b =>
        {
            b.HasOne("Domain.Permission", null).WithMany().HasForeignKey("PermissionId")
                .OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.HasOne("Domain.Role", null).WithMany().HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade).IsRequired();
        });

        modelBuilder.Entity("UserRoles", b =>
        {
            b.HasOne("Domain.Role", null).WithMany().HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.HasOne("Domain.User", null).WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade).IsRequired();
        });
    }
}
