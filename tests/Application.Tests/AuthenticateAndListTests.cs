using Domain;
using Xunit;

namespace Application.Tests;

public sealed class AuthenticateAndListTests
{
    [Theory]
    [InlineData("missing@demo.com", "correct")]
    [InlineData("original@demo.com", "wrong")]
    [InlineData("inactive@demo.com", "correct")]
    public async Task Invalid_credentials_or_inactive_user_do_not_issue_token(string email, string password)
    {
        var tokens = new FakeTokens();
        var repository = new FakeUserRepository(FakeUserRepository.CreateUser(), FakeUserRepository.CreateUser("inactive@demo.com", false));
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            new AuthenticateUser(repository, new FakePasswords(), tokens).ExecuteAsync(email, password, default));
        Assert.Equal(0, tokens.Calls);
    }

    [Fact]
    public async Task Active_user_with_correct_password_gets_token()
    {
        var tokens = new FakeTokens();
        var result = await new AuthenticateUser(new FakeUserRepository(FakeUserRepository.CreateUser()), new FakePasswords(), tokens)
            .ExecuteAsync(" ORIGINAL@DEMO.COM ", "correct", default);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(1, tokens.Calls);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task Rejects_invalid_pagination(int page, int pageSize)
    {
        await Assert.ThrowsAsync<InvalidInputException>(() =>
            new ListUsers(new FakeUserRepository()).ExecuteAsync(page, pageSize, default));
    }

    [Fact]
    public async Task Returns_requested_page_and_total_count()
    {
        var repository = new FakeUserRepository(FakeUserRepository.CreateUser("uno@demo.com"),
            FakeUserRepository.CreateUser("dos@demo.com"), FakeUserRepository.CreateUser("tres@demo.com"));
        var result = await new ListUsers(repository).ExecuteAsync(2, 2, default);
        Assert.Single(result.Items);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    private sealed class FakePasswords : IPasswordHasher
    {
        public string Hash(string password) => "test-hash";
        public bool Verify(string password, string hash) => password == "correct";
    }

    private sealed class FakeTokens : ITokenGenerator
    {
        public int Calls { get; private set; }
        public TokenDto Generate(User user) { Calls++; return new("fake-token", "Bearer", 1800); }
    }
}
