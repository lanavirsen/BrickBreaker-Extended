using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Services;

// Provides application-level leaderboard workflows independently of persistence concerns.
public sealed class LeaderboardService : ILeaderboardService
{
    private readonly ILeaderboardStore _store;

    public LeaderboardService(ILeaderboardStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public void Submit(ScoreEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        _store.Add(entry);
    }

    public void Submit(string username, int score)
    {
        if (string.IsNullOrWhiteSpace(username) || score < 0)
        {
            return;
        }

        _store.Add(new ScoreEntry(username.Trim(), score, DateTimeOffset.UtcNow));
    }

    public List<ScoreEntry> Top(int count)
    {
        if (count <= 0)
        {
            return [];
        }

        var entries = _store.ReadAll();
        return entries
            .Where(s => !string.IsNullOrWhiteSpace(s.Username) && s.Score >= 0)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.At)
            .ThenBy(s => s.Username, StringComparer.OrdinalIgnoreCase)
            .Take(count)
            .ToList();
    }

    public ScoreEntry? BestFor(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var entries = _store.ReadAll();
        return entries
            .Where(s => !string.IsNullOrWhiteSpace(s.Username) &&
                        s.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.At)
            .ThenBy(s => s.Username, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }
}
