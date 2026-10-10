using Domain;
using Xunit;

namespace Application.Tests;

public sealed class UpdateUserTests
{
    [Fact]
    public async Task Updates_fields_normalizes_email_and_preserves_hash_and_roles()
    {
        var user = FakeUserRepository.CreateUser();
        var repository = new FakeUserRepository(user);
        using var cancellation = new CancellationTokenSource();
        var result = await new UpdateUser(repository).ExecuteAsync(
            new(user.Id, "  Nuevo nombre  ", " NUEVO@DEMO.COM ", false), cancellation.Token);

        Assert.Equal(new UserDto(user.Id, "Nuevo nombre", "nuevo@demo.com", false), result);
        Assert.Equal("test-hash", user.PasswordHash);
        Assert.Single(user.Roles);
        Assert.Equal(1, repository.SaveCalls);
        Assert.Equal(cancellation.Token, repository.LastCancellationToken);
    }

    [Fact]
    public async Task Allows_keeping_own_email()
    {
        var user = FakeUserRepository.CreateUser();
        var repository = new FakeUserRepository(user);
        await new UpdateUser(repository).ExecuteAsync(new(user.Id, "Nuevo", "ORIGINAL@DEMO.COM", true), default);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Fact]
    public async Task Rejects_another_users_email_without_mutating_user()
    {
        var user = FakeUserRepository.CreateUser();
        var repository = new FakeUserRepository(user, FakeUserRepository.CreateUser("ocupado@demo.com"));
        await Assert.ThrowsAsync<EmailConflictException>(() => new UpdateUser(repository).ExecuteAsync(
            new(user.Id, "Nuevo", " OCUPADO@DEMO.COM ", false), default));
        Assert.Equal("Original", user.Name);
        Assert.Equal("original@demo.com", user.Email);
        Assert.True(user.IsActive);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Throws_when_user_does_not_exist()
    {
        var repository = new FakeUserRepository();
        await Assert.ThrowsAsync<UserNotFoundException>(() => new UpdateUser(repository).ExecuteAsync(
            new(Guid.NewGuid(), "Nuevo", "nuevo@demo.com", true), default));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sin-arroba")]
    [InlineData("Nombre <email@demo.com>")]
    public async Task Rejects_invalid_email(string email)
    {
        var user = FakeUserRepository.CreateUser();
        var repository = new FakeUserRepository(user);
        await Assert.ThrowsAsync<DomainValidationException>(() => new UpdateUser(repository).ExecuteAsync(
            new(user.Id, "Nuevo", email, true), default));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rejects_empty_name(string name)
    {
        var user = FakeUserRepository.CreateUser();
        var repository = new FakeUserRepository(user);
        await Assert.ThrowsAsync<InvalidInputException>(() => new UpdateUser(repository).ExecuteAsync(
            new(user.Id, name, user.Email, true), default));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Rejects_empty_user_identifier()
    {
        var repository = new FakeUserRepository();
        await Assert.ThrowsAsync<InvalidInputException>(() => new UpdateUser(repository).ExecuteAsync(
            new(Guid.Empty, "Nuevo", "nuevo@demo.com", true), default));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Rejects_name_over_storage_limit()
    {
        var user = FakeUserRepository.CreateUser();
        await Assert.ThrowsAsync<InvalidInputException>(() => new UpdateUser(new FakeUserRepository(user)).ExecuteAsync(
            new(user.Id, new string('a', 101), user.Email, true), default));
    }

    [Fact]
    public async Task Propagates_concurrent_email_conflict_from_persistence()
    {
        var user = FakeUserRepository.CreateUser();
        var repository = new FakeUserRepository(user) { SaveException = new EmailConflictException() };
        await Assert.ThrowsAsync<EmailConflictException>(() => new UpdateUser(repository).ExecuteAsync(
            new(user.Id, "Nuevo", "libre@demo.com", true), default));
    }

    [Fact]
    public async Task Honors_cancellation_without_saving()
    {
        var user = FakeUserRepository.CreateUser();
        var repository = new FakeUserRepository(user);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new UpdateUser(repository).ExecuteAsync(
            new(user.Id, "Nuevo", user.Email, true), cancellation.Token));
        Assert.Equal(0, repository.SaveCalls);
    }
}
