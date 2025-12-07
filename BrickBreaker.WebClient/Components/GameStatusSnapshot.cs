namespace BrickBreaker.WebClient.Components;

public sealed record GameStatusSnapshot(int Score, int Level, int ActiveBalls, bool IsPaused, bool IsGameOver, bool BallReady);
