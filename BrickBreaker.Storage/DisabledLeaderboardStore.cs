using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;
using System.Collections.Generic;

namespace BrickBreaker.Storage;

// Fallback implementation that no-ops when the Supabase connection string is missing.
public sealed class DisabledLeaderboardStore : ILeaderboardStore
{
    public void Add(ScoreEntry entry)
    {
        // Intentionally does nothing - leaderboard persistence requires the database.
    }

    public List<ScoreEntry> ReadAll() => new();
}
