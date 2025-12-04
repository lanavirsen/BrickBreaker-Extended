namespace BrickBreaker.Core.Abstractions;

/// <summary>
/// Exposes authentication-related use cases for UI layers.
/// </summary>
public interface IAuthService
{
    bool UsernameExists(string username);

    bool Register(string username, string password);

    bool Login(string username, string password);
}
