namespace BrickBreaker.Core.Clients;

/// <summary>
/// Shared abstraction for launching a BrickBreaker gameplay session and returning the final score.
/// </summary>
public interface IGameHost
{
    int Run();
}
