using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Services;

using System.Threading.Tasks;

// Application-layer service that encapsulates user registration and login workflows.
public sealed class AuthService : IAuthService
{
    private readonly IUserStore _users;
    private readonly IProfanityFilter _profanity;

    public AuthService(IUserStore users, IProfanityFilter profanityFilter)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _profanity = profanityFilter ?? throw new ArgumentNullException(nameof(profanityFilter));
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        username = (username ?? string.Empty).Trim();
        return await _users.ExistsAsync(username).ConfigureAwait(false);
    }

    public async Task<RegisterResult> RegisterAsync(string username, string password)
    {
        username = (username ?? string.Empty).Trim();
        if (username.Length == 0)
        {
            return RegisterResult.Fail("username_required");
        }

        if (_profanity.ContainsProfanity(username))
        {
            return RegisterResult.Fail("username_profane");
        }

        if (await _users.ExistsAsync(username).ConfigureAwait(false))
        {
            return RegisterResult.Fail("username_taken");
        }

        password = (password ?? string.Empty).Trim();
        if (password.Length < 5)
        {
            return RegisterResult.Fail("password_too_short");
        }

        var hashedPassword = PasswordHasher.HashPassword(password);
        await _users.AddAsync(new User(username, hashedPassword)).ConfigureAwait(false);
        return RegisterResult.Ok();
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        username = (username ?? string.Empty).Trim();
        var storedUser = await _users.GetAsync(username).ConfigureAwait(false);
        return storedUser is not null && PasswordHasher.Verify(storedUser.Password, password ?? string.Empty);
    }
}
