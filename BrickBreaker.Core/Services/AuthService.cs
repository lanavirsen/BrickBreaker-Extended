using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Services;

// Application-layer service that encapsulates user registration and login workflows.
public sealed class AuthService : IAuthService
{
    private readonly IUserStore _users;
    private readonly IProfanityFilter _profanity;

    // Constructor. It runs whenever the DI container creates an AuthService.
    public AuthService(IUserStore users, IProfanityFilter profanityFilter)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _profanity = profanityFilter ?? throw new ArgumentNullException(nameof(profanityFilter));
    }

    // Helper used by UI layers to drive instant username availability checks.
    public async Task<bool> UsernameExistsAsync(string username)
    {
        username = (username ?? string.Empty).Trim();
        return await _users.ExistsAsync(username).ConfigureAwait(false);
    }

    /*
    ConfigureAwait(false) tells the awaiter not to capture the current synchronization context when resuming the async
    method. In UI frameworks (WinForms, WPF) this avoids marshaling back to the UI thread and can prevent deadlocks.
    In ASP.NET Core there’s no synchronization context by default, so it doesn’t usually change behavior, but many
    libraries keep it as a habit to signal “resume on any thread”. In my UsernameExistsAsync, it simply means after
    _users.ExistsAsync completes, the continuation can run on whatever thread is available instead of trying to get back
    to the original context.
    */

    public async Task<RegisterResult> RegisterAsync(string username, string password)
    {
        username = (username ?? string.Empty).Trim();

        // Username validation stays close to the domain so each client (API, console, WinForms) gets identical rules.
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

        // Password policy intentionally small, but centralized here so we can evolve it (length, complexity, etc.) once.
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

        // Password verification happens against the hashed value stored in persistence.
        return storedUser is not null && PasswordHasher.Verify(storedUser.Password, password ?? string.Empty);
    }
}
