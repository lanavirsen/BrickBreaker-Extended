namespace BrickBreaker.Core.Clients;

public sealed record LauncherViewState(
    string PlayerLabel,
    string BestScoreLabel,
    bool CanStartGame,
    bool CanLogout,
    bool IsQuickPlay,
    string StatusMessage,
    bool StatusIsSuccess);
