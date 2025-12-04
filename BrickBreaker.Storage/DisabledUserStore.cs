using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Storage;

// Fallback implementation that keeps the app running when the database connection is unavailable.
public sealed class DisabledUserStore : IUserStore
{
    public bool Exists(string username) => false;

    public void Add(User user)
    {
        // Intentionally left blank - database-backed registration is disabled.
    }

    public User? Get(string username) => null;
}
