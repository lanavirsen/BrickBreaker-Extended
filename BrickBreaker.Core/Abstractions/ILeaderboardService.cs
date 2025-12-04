using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Abstractions;

/// <summary>
/// Provides leaderboard operations for presentation layers.
/// </summary>
public interface ILeaderboardService
{
    void Submit(ScoreEntry entry);
    void Submit(string username, int score);
    List<ScoreEntry> Top(int count);
    ScoreEntry? BestFor(string username);
}
