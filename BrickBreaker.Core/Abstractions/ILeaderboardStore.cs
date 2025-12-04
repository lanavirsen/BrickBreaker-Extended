using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Abstractions;

public interface ILeaderboardStore
{
    Task AddAsync(ScoreEntry entry, CancellationToken cancellationToken = default);
    Task<List<ScoreEntry>> ReadAllAsync(CancellationToken cancellationToken = default);
}
