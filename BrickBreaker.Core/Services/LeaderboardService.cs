using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Services;

// Provides application-level leaderboard workflows independently of persistence concerns.
public sealed class LeaderboardService : ILeaderboardService
{
    // Scores above this threshold are considered unrealistic and rejected server-side.
    private const int MaxAllowedScore = 1_000_000;
    private readonly ILeaderboardStore _store;

    public LeaderboardService(ILeaderboardStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task SubmitAsync(ScoreEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (!IsScoreWithinBounds(entry.Score) || string.IsNullOrWhiteSpace(entry.Username))
        {
            return;
        }

        await _store.AddAsync(entry).ConfigureAwait(false);
    }

    public async Task SubmitAsync(string username, int score)
    {
        if (string.IsNullOrWhiteSpace(username) || !IsScoreWithinBounds(score))
        {
            return;
        }

        await _store.AddAsync(new ScoreEntry(username.Trim(), score, DateTimeOffset.UtcNow)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ScoreEntry>> TopAsync(int count)
    {
        if (count <= 0)
        {
            return [];
        }

        var entries = await _store.ReadTopAsync(count).ConfigureAwait(false);
        return entries.Where(s => !string.IsNullOrWhiteSpace(s.Username) && IsScoreWithinBounds(s.Score)).ToList();
    }

    public async Task<ScoreEntry?> BestForAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return await _store.ReadBestForAsync(username.Trim()).ConfigureAwait(false);
    }

    private static bool IsScoreWithinBounds(int score) => score >= 0 && score <= MaxAllowedScore;
}
