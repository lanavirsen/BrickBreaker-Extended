namespace BrickBreaker.Core.Abstractions;

using System.Threading.Tasks;

// Exposes authentication-related use cases for UI layers.
public interface IAuthService
{
    Task<bool> UsernameExistsAsync(string username);

    Task<bool> RegisterAsync(string username, string password);

    Task<bool> LoginAsync(string username, string password);
}
