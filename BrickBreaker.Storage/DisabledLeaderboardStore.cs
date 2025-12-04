using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Storage;

// Fallback implementation that no-ops when the Supabase connection string is missing.
public sealed class DisabledLeaderboardStore : ILeaderboardStore
{
    public Task AddAsync(ScoreEntry entry, CancellationToken cancellationToken = default)
    {
        // Intentionally does nothing - leaderboard persistence requires the database.
        return Task.CompletedTask;
    }

    public Task<List<ScoreEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new List<ScoreEntry>());
}
