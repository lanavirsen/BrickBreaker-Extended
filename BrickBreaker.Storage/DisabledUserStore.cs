using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Storage;

// Fallback implementation that keeps the app running when the database connection is unavailable.
public sealed class DisabledUserStore : IUserStore
{
    public Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        // Intentionally left blank - database-backed registration is disabled.
        return Task.CompletedTask;
    }

    public Task<User?> GetAsync(string username, CancellationToken cancellationToken = default)
        => Task.FromResult<User?>(null);
}
