using System.Collections.Generic;
using System.Threading.Tasks;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Abstractions;

// Provides leaderboard operations for presentation layers.
public interface ILeaderboardService
{
    Task SubmitAsync(ScoreEntry entry);
    Task SubmitAsync(string username, int score);
    Task<IReadOnlyList<ScoreEntry>> TopAsync(int count);
    Task<ScoreEntry?> BestForAsync(string username);
}
