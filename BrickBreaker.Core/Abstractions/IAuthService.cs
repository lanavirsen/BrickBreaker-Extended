namespace BrickBreaker.Core.Abstractions;

// Exposes authentication-related use cases for UI layers.
public interface IAuthService
{
    bool UsernameExists(string username);

    bool Register(string username, string password);

    bool Login(string username, string password);
}
