using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Abstractions;

public interface ILeaderboardStore
{
    void Add(ScoreEntry entry);
    List<ScoreEntry> ReadAll();
}
