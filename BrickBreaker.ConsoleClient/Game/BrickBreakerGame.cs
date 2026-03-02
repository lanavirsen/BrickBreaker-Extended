using System.Diagnostics;
using System.Drawing;
using System.Text;
using BrickBreaker.ConsoleClient.Game.Infrastructure;
using BrickBreaker.ConsoleClient.Game.Systems;
using BrickBreaker.Core.Clients;
using BrickBreaker.Game.Utilities;
using BrickBreaker.Gameplay;

namespace BrickBreaker.ConsoleClient.Game;

public sealed class BrickBreakerGame : IGameHost
{
    private readonly IKeyboard _keyboard;
    private readonly ConsoleRenderer _renderer = new();

    public BrickBreakerGame(IKeyboard keyboard)
    {
        _keyboard = keyboard;
    }

    public BrickBreakerGame() : this(new Win32Keyboard()) { }

    public int Run()
    {
        // Play area: (InitialBrickCols-1) spacings wide + one brick + margin on each side
        int boardW = (GameConstants.InitialBrickCols - 1) * GameConstants.BrickXSpacing + GameConstants.BrickWidth;
        int boardH = (GameConstants.InitialBrickRows - 1) * GameConstants.BrickYSpacing + GameConstants.BrickHeight + GameConstants.PaddleAreaHeight;
        var playArea = new Rectangle(0, 0, boardW + GameConstants.PlayAreaMargin * 2, boardH);

        var session = new GameSession();
        session.Initialize(playArea);

        try { Console.CursorVisible = false; } catch { }
        Console.OutputEncoding = Encoding.UTF8;
        Console.TreatControlCAsInput = true;

        var sw = new Stopwatch();
        sw.Start();
        var targetDt = TimeSpan.FromMilliseconds(1000.0 / 60.0);
        var last = sw.Elapsed;

        bool prevSpaceDown = false;
        bool prevUpDown = false;

        while (!session.Snapshot.IsGameOver)
        {
            var now = sw.Elapsed;
            while (now - last >= targetDt)
            {
                // Drain the key-event buffer so input reads are current
                while (Console.KeyAvailable) Console.ReadKey(true);

                // Escape → early exit
                if (_keyboard.IsEscapePressed())
                {
                    try { Console.CursorVisible = true; } catch { }
                    return session.Snapshot.Score;
                }

                // Space → toggle pause (edge-detect)
                bool spaceDown = _keyboard.IsSpacePressed();
                if (spaceDown && !prevSpaceDown)
                    session.TogglePause();
                prevSpaceDown = spaceDown;

                // Left / Right → paddle movement
                session.SetInput(_keyboard.IsLeftPressed(), _keyboard.IsRightPressed());

                // Up → launch ball (edge-detect, only when BallReady)
                bool upDown = _keyboard.IsUpPressed();
                if (upDown && !prevUpDown && session.Snapshot.BallReady)
                    session.TryLaunchBall();
                prevUpDown = upDown;

                session.Update(targetDt.TotalSeconds);
                last += targetDt;
            }

            _renderer.Render(session.Snapshot);

            var sleep = targetDt - (sw.Elapsed - now);
            if (sleep > TimeSpan.Zero)
                Thread.Sleep(sleep);
        }

        try { Console.CursorVisible = true; } catch { }
        return session.Snapshot.Score;
    }
}
