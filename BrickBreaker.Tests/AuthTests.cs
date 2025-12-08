using System.Threading.Tasks;
using BrickBreaker.Core.Models;
using BrickBreaker.Core.Services;

namespace BrickBreaker.Tests;

public sealed class AuthTests
{
    [Fact]
    public async Task UsernameExists_TrimsInputBeforeDelegating()
    {
        var store = new FakeUserStore();
        store.Seed(new User("Alice", "secret"));
        var sut = CreateAuthService(store);

        var exists = await sut.UsernameExistsAsync("  Alice  ");

        Assert.True(exists);
        Assert.Equal("Alice", store.LastExistsUsername);
    }

    [Fact]
    public async Task Register_Fails_WhenUsernameAlreadyExists()
    {
        // Username is already seeded, so Register should reject the duplicate.
        var store = new FakeUserStore();
        store.Seed(CreateHashedUser("Bob", "pw"));
        var sut = CreateAuthService(store);

        var result = await sut.RegisterAsync("Bob", "newpw");

        Assert.False(result);
        Assert.Empty(store.AddedUsers);
    }

    [Fact]
    public async Task Register_Fails_WhenUsernameMissing()
    {
        // Whitespace usernames are invalid.
        var sut = CreateAuthService();

        var result = await sut.RegisterAsync("   ", "pw");

        Assert.False(result);
    }

    [Fact]
    public async Task Register_Fails_WhenPasswordMissing()
    {
        // Password cannot be empty, so the call should fail.
        var sut = CreateAuthService();

        var result = await sut.RegisterAsync("Alice", "");

        Assert.False(result);
    }

    [Fact]
    public async Task Register_Fails_WhenPasswordTooShort()
    {
        var sut = CreateAuthService();

        var result = await sut.RegisterAsync("Alice", "1234");

        Assert.False(result);
    }

    [Fact]
    public async Task Register_Succeeds_AndPersistsTrimmedUser()
    {
        // Valid username/password should succeed and be stored without extra whitespace.
        var store = new FakeUserStore();
        var sut = CreateAuthService(store);

        var result = await sut.RegisterAsync("  Alice  ", "  strongpw  ");

        Assert.True(result);
        var savedUser = Assert.Single(store.AddedUsers);
        Assert.Equal("Alice", savedUser.Username);
        Assert.True(PasswordHasher.Verify(savedUser.Password, "strongpw"));
    }

    [Fact]
    public async Task Login_ReturnsFalse_WhenUserMissing()
    {
        // If the backing store does not contain the user, login should fail.
        var sut = CreateAuthService();

        var loggedIn = await sut.LoginAsync("unknown", "pw");

        Assert.False(loggedIn);
    }

    [Fact]
    public async Task Login_ReturnsFalse_WhenPasswordMismatch()
    {
        // Valid username but wrong password should fail to log in.
        var store = new FakeUserStore();
        store.Seed(CreateHashedUser("Alice", "pw"));
        var sut = CreateAuthService(store);

        var loggedIn = await sut.LoginAsync("Alice", "wrong");

        Assert.False(loggedIn);
    }

    [Fact]
    public async Task Login_ReturnsTrue_WhenCredentialsMatch()
    {
        // Exact match of username/password should succeed.
        var store = new FakeUserStore();
        store.Seed(CreateHashedUser("Alice", "pw"));
        var sut = CreateAuthService(store);

        var loggedIn = await sut.LoginAsync("Alice", "pw");

        Assert.True(loggedIn);
    }

    [Fact]
    public async Task Login_TrimsUsernameBeforeLookup()
    {
        // Ensure the repository is called with a trimmed username.
        var store = new FakeUserStore();
        store.Seed(CreateHashedUser("Alice", "pw"));
        var sut = CreateAuthService(store);

        _ = await sut.LoginAsync("  Alice  ", "pw");

        Assert.Equal("Alice", store.LastGetUsername);
    }

    [Fact]
    public async Task Register_Fails_WhenProfanityDetected()
    {
        var store = new FakeUserStore();
        var filter = new FakeProfanityFilter { ShouldFlag = true };
        var sut = CreateAuthService(store, filter);

        var result = await sut.RegisterAsync("OffensiveName", "pw");

        Assert.False(result);
        Assert.Empty(store.AddedUsers);
        Assert.Equal("OffensiveName", filter.LastChecked);
    }

    private static User CreateHashedUser(string username, string password)
    {
        var hashed = PasswordHasher.HashPassword(password);
        return new User(username, hashed);
    }

    private static AuthService CreateAuthService(FakeUserStore? store = null, FakeProfanityFilter? filter = null)
    {
        return new AuthService(store ?? new FakeUserStore(), filter ?? new FakeProfanityFilter());
    }
}
