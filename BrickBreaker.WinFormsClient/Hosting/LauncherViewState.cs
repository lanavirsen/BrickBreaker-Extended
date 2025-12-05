namespace BrickBreaker.WinFormsClient.Hosting;

public sealed record LauncherViewState(
    string PlayerLabel,
    string LastScoreLabel,
    bool CanStartGame,
    bool CanLogout,
    bool IsQuickPlay,
    string StatusMessage,
    bool StatusIsSuccess);
