namespace BrickBreaker.Core.Abstractions;

using System.Threading.Tasks;
using BrickBreaker.Core.Models;

// Exposes authentication-related use cases for UI layers.
public interface IAuthService
{
    Task<bool> UsernameExistsAsync(string username);

    Task<RegisterResult> RegisterAsync(string username, string password);

    Task<bool> LoginAsync(string username, string password);
}
