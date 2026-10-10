using Application;
using Domain;

namespace Application.Tests;

internal sealed class FakeUserRepository(params User[] initialUsers) : IUserRepository
{
    public List<User> Users { get; } = [.. initialUsers];
    public int SaveCalls { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }
    public Exception? SaveException { get; set; }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Users.SingleOrDefault(x => x.Id == id));
    }

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(x => x.Email == normalizedEmail));

    public Task<bool> EmailExistsAsync(string normalizedEmail, Guid exceptUserId, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Any(x => x.Email == normalizedEmail && x.Id != exceptUserId));

    public Task<PagedUsers> ListAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        Task.FromResult(new PagedUsers(Users.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new UserDto(x.Id, x.Name, x.Email, x.IsActive)).ToList(), page, pageSize, Users.Count));

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastCancellationToken = cancellationToken;
        if (SaveException is not null) return Task.FromException(SaveException);
        SaveCalls++;
        return Task.CompletedTask;
    }

    public static User CreateUser(string email = "original@demo.com", bool active = true) =>
        new(Guid.NewGuid(), "Original", email, "test-hash", active,
            [new Role(1, "Cualquier nombre", [new Permission(1, Permissions.UsersUpdate)])]);
}
