using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Abstractions;

public interface IUserStore
{
    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetAsync(string username, CancellationToken cancellationToken = default);
}
