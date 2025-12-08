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

    public Task<List<ScoreEntry>> ReadTopAsync(int count, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<ScoreEntry>());

    public Task<ScoreEntry?> ReadBestForAsync(string username, CancellationToken cancellationToken = default)
        => Task.FromResult<ScoreEntry?>(null);
}
