using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Abstractions;

// Provides leaderboard operations for presentation layers.
public interface ILeaderboardService
{
    void Submit(ScoreEntry entry);
    void Submit(string username, int score);
    List<ScoreEntry> Top(int count);
    ScoreEntry? BestFor(string username);
}
