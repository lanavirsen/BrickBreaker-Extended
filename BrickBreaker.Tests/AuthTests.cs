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
        var sut = new AuthService(store);

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
        var sut = new AuthService(store);

        var result = await sut.RegisterAsync("Bob", "newpw");

        Assert.False(result);
        Assert.Empty(store.AddedUsers);
    }

    [Fact]
    public async Task Register_Fails_WhenUsernameMissing()
    {
        // Whitespace usernames are invalid.
        var sut = new AuthService(new FakeUserStore());

        var result = await sut.RegisterAsync("   ", "pw");

        Assert.False(result);
    }

    [Fact]
    public async Task Register_Fails_WhenPasswordMissing()
    {
        // Password cannot be empty, so the call should fail.
        var sut = new AuthService(new FakeUserStore());

        var result = await sut.RegisterAsync("Alice", "");

        Assert.False(result);
    }

    [Fact]
    public async Task Register_Succeeds_AndPersistsTrimmedUser()
    {
        // Valid username/password should succeed and be stored without extra whitespace.
        var store = new FakeUserStore();
        var sut = new AuthService(store);

        var result = await sut.RegisterAsync("  Alice  ", "  pw  ");

        Assert.True(result);
        var savedUser = Assert.Single(store.AddedUsers);
        Assert.Equal("Alice", savedUser.Username);
        Assert.True(PasswordHasher.Verify(savedUser.Password, "pw"));
    }

    [Fact]
    public async Task Login_ReturnsFalse_WhenUserMissing()
    {
        // If the backing store does not contain the user, login should fail.
        var sut = new AuthService(new FakeUserStore());

        var loggedIn = await sut.LoginAsync("unknown", "pw");

        Assert.False(loggedIn);
    }

    [Fact]
    public async Task Login_ReturnsFalse_WhenPasswordMismatch()
    {
        // Valid username but wrong password should fail to log in.
        var store = new FakeUserStore();
        store.Seed(CreateHashedUser("Alice", "pw"));
        var sut = new AuthService(store);

        var loggedIn = await sut.LoginAsync("Alice", "wrong");

        Assert.False(loggedIn);
    }

    [Fact]
    public async Task Login_ReturnsTrue_WhenCredentialsMatch()
    {
        // Exact match of username/password should succeed.
        var store = new FakeUserStore();
        store.Seed(CreateHashedUser("Alice", "pw"));
        var sut = new AuthService(store);

        var loggedIn = await sut.LoginAsync("Alice", "pw");

        Assert.True(loggedIn);
    }

    [Fact]
    public async Task Login_TrimsUsernameBeforeLookup()
    {
        // Ensure the repository is called with a trimmed username.
        var store = new FakeUserStore();
        store.Seed(CreateHashedUser("Alice", "pw"));
        var sut = new AuthService(store);

        _ = await sut.LoginAsync("  Alice  ", "pw");

        Assert.Equal("Alice", store.LastGetUsername);
    }

    private static User CreateHashedUser(string username, string password)
    {
        var hashed = PasswordHasher.HashPassword(password);
        return new User(username, hashed);
    }
}
