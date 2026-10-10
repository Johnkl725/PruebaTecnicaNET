using Application;
using Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class UserRepository(UsersDbContext db) : IUserRepository
{
    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().Include(x => x.Roles).ThenInclude(x => x.Permissions)
            .SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, Guid exceptUserId, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(x => x.Email == normalizedEmail && x.Id != exceptUserId, cancellationToken);

    public async Task<PagedUsers> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new UserDto(x.Id, x.Name, x.Email, x.IsActive)).ToListAsync(cancellationToken);
        return new PagedUsers(items, page, pageSize, total);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 } &&
            exception.Entries.All(entry => entry.Entity is User && entry.State == EntityState.Modified))
        {
            throw new EmailConflictException();
        }
    }
}
