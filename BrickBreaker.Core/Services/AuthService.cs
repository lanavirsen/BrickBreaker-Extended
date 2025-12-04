using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Services;

/// <summary>
/// Application-layer service that encapsulates user registration and login workflows.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserStore _users;

    public AuthService(IUserStore users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public bool UsernameExists(string username)
    {
        username = (username ?? string.Empty).Trim();
        return _users.Exists(username);
    }

    public bool Register(string username, string password)
    {
        username = (username ?? string.Empty).Trim();
        if (_users.Exists(username) || username.Length == 0)
        {
            return false;
        }

        password = (password ?? string.Empty).Trim();
        if (password.Length == 0)
        {
            return false;
        }

        var hashedPassword = PasswordHasher.HashPassword(password);
        _users.Add(new User(username, hashedPassword));
        return true;
    }

    public bool Login(string username, string password)
    {
        username = (username ?? string.Empty).Trim();
        var storedUser = _users.Get(username);
        return storedUser is not null && PasswordHasher.Verify(storedUser.Password, password ?? string.Empty);
    }
}
