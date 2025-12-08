using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Game.Utilities;
using BrickBreaker.Gameplay;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BrickBreaker.WebClient.Services;

public sealed class WebGameHost : IAsyncDisposable
{
    private static readonly int BoardWidth = (GameConstants.InitialBrickCols - 1) * GameConstants.BrickXSpacing + GameConstants.BrickWidth;
    private static readonly int BoardHeight = (GameConstants.InitialBrickRows - 1) * GameConstants.BrickYSpacing + GameConstants.BrickHeight + GameConstants.PaddleAreaHeight;
    private const int PaddleHeight = 20;
    public static readonly int CanvasWidth = BoardWidth + GameConstants.PlayAreaMargin * 2;
    public static readonly int CanvasHeight = BoardHeight + GameConstants.PlayAreaMargin;

    private readonly IJSRuntime _jsRuntime;
    private readonly GameSession _session = new();
    private IJSObjectReference? _canvasModule;
    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private DateTime _lastFrame;
    private bool _leftPressed;
    private bool _rightPressed;
    private int _lastScore;
    private int _lastLevel = 1;
    private bool _lastPaused;
    private bool _lastGameOver;
    private bool _lastBallReady = true;

    public event Action? StateChanged;
    public event Action<int>? GameFinished;

    public WebGameHost(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _session.GameFinished += (_, score) =>
        {
            GameFinished?.Invoke(score);
            NotifyStateChanged();
        };
    }

    private GameRenderState State => _session.Snapshot;

    public int Score => State.Score;
    public int Level => State.Level;
    public int ActiveBalls => State.Balls.Count;
    public bool IsPaused => State.IsPaused;
    public bool IsGameOver => State.IsGameOver;
    public bool BallReady => State.BallReady;

    public async Task InitializeAsync(ElementReference canvas)
    {
        if (_canvasModule is null)
        {
            _canvasModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/gameCanvas.js");
        }

        await _canvasModule.InvokeVoidAsync("init", canvas, CanvasWidth, CanvasHeight);

        _session.Initialize(new Rectangle(0, 0, CanvasWidth, CanvasHeight));
        _lastFrame = DateTime.UtcNow;
        StartLoop();
        NotifyStateChanged();
    }

    private void StartLoop()
    {
        _loopCts?.Cancel();
        _loopCts = new CancellationTokenSource();
        _loopTask = RunLoopAsync(_loopCts.Token);
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        var targetFrame = TimeSpan.FromMilliseconds(16);
        while (!token.IsCancellationRequested)
        {
            var frameStart = DateTime.UtcNow;
            var delta = (frameStart - _lastFrame).TotalSeconds;
            _lastFrame = frameStart;

            _session.Update(delta);
            BroadcastChanges();
            if (_canvasModule is not null)
            {
                await RenderAsync();
            }

            var elapsed = DateTime.UtcNow - frameStart;
            var delay = targetFrame - elapsed;
            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            else
            {
                await Task.Yield();
            }
        }
    }

    public void SetLeftPressed(bool pressed)
    {
        _leftPressed = pressed;
        _session.SetInput(_leftPressed, _rightPressed);
    }

    public void SetRightPressed(bool pressed)
    {
        _rightPressed = pressed;
        _session.SetInput(_leftPressed, _rightPressed);
    }

    public void ClearInput()
    {
        _leftPressed = false;
        _rightPressed = false;
        _session.SetInput(false, false);
    }

    public void TryLaunchBall() => _session.TryLaunchBall();

    public void TogglePause()
    {
        _session.TogglePause();
        BroadcastChanges();
    }

    public void Restart()
    {
        _session.Restart();
        BroadcastChanges();
    }

    private async Task RenderAsync()
    {
        if (_canvasModule is null)
        {
            return;
        }

        var state = State;
        var payload = new
        {
            playArea = ToRect(state.PlayArea),
            paddle = new { x = (int)state.PaddleX, y = state.PaddleY, width = state.PaddleWidth, height = PaddleHeight },
            balls = state.Balls.Select(b => new { x = b.X, y = b.Y, radius = b.Radius }).ToArray(),
            bricks = state.Bricks.Select(b => new { x = b.X, y = b.Y, width = b.Width, height = b.Height, color = ToColor(b.Color) }).ToArray(),
            powerUps = state.PowerUps.Select(p => new { x = p.X, y = p.Y, width = p.Width, height = p.Height, kind = p.Type.ToString() }).ToArray(),
            scorePopups = state.ScorePopups.Select(p => new { x = p.X, y = p.Y, text = p.Text, opacity = p.Opacity, isMultiplier = p.IsMultiplier }).ToArray(),
            overlay = new { score = state.Score, highScore = state.HighScore, level = state.Level, borderHue = state.BorderHue, isPaused = state.IsPaused, isGameOver = state.IsGameOver, ballReady = state.BallReady }
        };

        await _canvasModule.InvokeVoidAsync("render", payload);
    }

    private static object ToRect(Rectangle rect) => new { x = rect.X, y = rect.Y, width = rect.Width, height = rect.Height };

    private static object ToColor(Color color) => new { r = color.R, g = color.G, b = color.B };

    private void BroadcastChanges()
    {
        var state = State;
        if (state.Score != _lastScore || state.Level != _lastLevel || state.IsPaused != _lastPaused || state.IsGameOver != _lastGameOver || state.BallReady != _lastBallReady)
        {
            _lastScore = state.Score;
            _lastLevel = state.Level;
            _lastPaused = state.IsPaused;
            _lastGameOver = state.IsGameOver;
            _lastBallReady = state.BallReady;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        _loopCts?.Cancel();
        if (_canvasModule is not null)
        {
            try
            {
                await _canvasModule.InvokeVoidAsync("dispose");
            }
            catch
            {
                // ignore cleanup failures during disposal
            }
            await _canvasModule.DisposeAsync();
        }
    }
}
