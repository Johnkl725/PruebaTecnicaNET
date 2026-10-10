using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

internal sealed class DevelopmentSeeder(UsersDbContext db, IPasswordHasher passwords, IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var read = await db.Permissions.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (read is null) { read = new Permission(1, Application.Permissions.UsersRead); db.Permissions.Add(read); }
        var update = await db.Permissions.SingleOrDefaultAsync(x => x.Id == 2, cancellationToken);
        if (update is null) { update = new Permission(2, Application.Permissions.UsersUpdate); db.Permissions.Add(update); }
        var admin = await db.Roles.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (admin is null) { admin = new Role(1, "Administrador", [read, update]); db.Roles.Add(admin); }
        var reader = await db.Roles.SingleOrDefaultAsync(x => x.Id == 2, cancellationToken);
        if (reader is null) { reader = new Role(2, "Consulta", [read]); db.Roles.Add(reader); }

        await AddUserAsync("11111111-1111-1111-1111-111111111111", "Administrador", "admin@demo.com", true, admin, "Admin", cancellationToken);
        await AddUserAsync("22222222-2222-2222-2222-222222222222", "Consulta", "consulta@demo.com", true, reader, "Consulta", cancellationToken);
        await AddUserAsync("33333333-3333-3333-3333-333333333333", "Inactivo", "inactivo@demo.com", false, reader, "Inactivo", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task AddUserAsync(string id, string name, string email, bool active, Role role, string key, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(id);
        if (await db.Users.AnyAsync(x => x.Id == userId, cancellationToken)) return;
        var password = configuration[$"Seed:Passwords:{key}"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException($"Falta configurar Seed:Passwords:{key} para la carga de desarrollo.");
        db.Users.Add(new User(userId, name, email, passwords.Hash(password), active, [role]));
    }
}
