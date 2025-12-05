using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Game;
using BrickBreaker.Game.Entities;
using BrickBreaker.Game.Utilities;
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
    private readonly GameEngine _engine = new();
    private Rectangle _playArea = new(0, 0, CanvasWidth, CanvasHeight);
    private IJSObjectReference? _canvasModule;
    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private DateTime _lastFrame;

    private double _paddleX;
    private int _paddleY;
    private bool _ballReady = true;
    private bool _isPaused;
    private bool _isGameOver;
    private bool _leftPressed;
    private bool _rightPressed;
    private double _borderHue;

    public event Action? StateChanged;

    public WebGameHost(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _engine.ScoreChanged += (_, _) => NotifyStateChanged();
        _engine.GameOver += (_, _) =>
        {
            _isGameOver = true;
            _ballReady = true;
            NotifyStateChanged();
        };
        _engine.LevelLoaded += (_, _) =>
        {
            _ballReady = true;
            NotifyStateChanged();
        };
    }

    public int Score => _engine.Score;
    public int Level => _engine.CurrentLevel;
    public bool IsPaused => _isPaused;
    public bool IsGameOver => _isGameOver;
    public bool BallReady => _ballReady;

    public async Task InitializeAsync(ElementReference canvas)
    {
        if (_canvasModule is null)
        {
            _canvasModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/gameCanvas.js");
        }

        await _canvasModule.InvokeVoidAsync("init", canvas, CanvasWidth, CanvasHeight);

        ResetWorld();
        _lastFrame = DateTime.UtcNow;
        StartLoop();
        NotifyStateChanged();
    }

    private void ResetWorld()
    {
        _engine.StartLevel(1, _playArea);
        _paddleY = _playArea.Bottom - 40;
        _paddleX = _playArea.Left + (_playArea.Width - _engine.CurrentPaddleWidth) / 2;
        _isGameOver = false;
        _isPaused = false;
        _ballReady = true;
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

            Update(delta);
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

    private void Update(double deltaTime)
    {
        UpdatePaddleMovement();

        if (_isGameOver || _isPaused)
        {
            return;
        }

        UpdateBorder(deltaTime);

        if (_ballReady)
        {
            SnapBallToPaddle();
        }
        else
        {
            _engine.Update(deltaTime, _playArea, _paddleX, _paddleY);
        }
    }

    private void UpdatePaddleMovement()
    {
        if (_leftPressed && _paddleX > _playArea.Left)
        {
            _paddleX -= GameConstants.BasePaddleSpeed;
        }

        if (_rightPressed && _paddleX < _playArea.Right - _engine.CurrentPaddleWidth)
        {
            _paddleX += GameConstants.BasePaddleSpeed;
        }
    }

    private void UpdateBorder(double deltaTime)
    {
        _borderHue = (_borderHue + 120 * deltaTime) % 360;
    }

    private void SnapBallToPaddle()
    {
        if (_engine.Balls.Count == 0)
        {
            return;
        }

        var ball = _engine.Balls[0];
        ball.X = (int)(_paddleX + _engine.CurrentPaddleWidth / 2 - GameConstants.BallRadius);
        ball.Y = _paddleY - 40;
        ball.VX = 0;
        ball.VY = 0;
    }

    public void SetLeftPressed(bool pressed) => _leftPressed = pressed;
    public void SetRightPressed(bool pressed) => _rightPressed = pressed;

    public void ClearInput()
    {
        _leftPressed = false;
        _rightPressed = false;
    }

    public void TryLaunchBall()
    {
        if (_ballReady && _engine.Balls.Count > 0)
        {
            _engine.Balls[0].VY = -_engine.CurrentBallSpeed;
            _ballReady = false;
        }
    }

    public void TogglePause()
    {
        if (_isGameOver)
        {
            return;
        }

        _isPaused = !_isPaused;
        NotifyStateChanged();
    }

    public void Restart()
    {
        ResetWorld();
        NotifyStateChanged();
    }

    private async Task RenderAsync()
    {
        if (_canvasModule is null)
        {
            return;
        }

        var renderState = new GameRenderState(
            new RectState(_playArea.X, _playArea.Y, _playArea.Width, _playArea.Height),
            new PaddleState((int)_paddleX, _paddleY, _engine.CurrentPaddleWidth, PaddleHeight),
            _engine.Balls.Select(b => new BallState(b.X, b.Y, b.Radius)).ToArray(),
            _engine.Bricks.Where(b => b.IsVisible).Select(b => new BrickState(b.X, b.Y, b.Width, b.Height, ToColor(b.BrickColor))).ToArray(),
            _engine.PowerUps.Select(p => new PowerUpState(p.X, p.Y, p.Width, p.Height, p.Type.ToString())).ToArray(),
            _engine.ScorePopups.Select(p => new ScorePopupState(p.X, p.Y, p.DisplayText, p.Opacity, p.IsMultiplier)).ToArray(),
            new OverlayState(_engine.Score, _engine.HighScore, _engine.CurrentLevel, _borderHue, _isPaused, _isGameOver, _ballReady)
        );

        await _canvasModule.InvokeVoidAsync("render", renderState);
    }

    private static RgbColor ToColor(Color color) => new(color.R, color.G, color.B);

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        _loopCts?.Cancel();
        if (_canvasModule is not null)
        {
            await _canvasModule.DisposeAsync();
        }
    }

    private sealed record GameRenderState(
        RectState playArea,
        PaddleState paddle,
        BallState[] balls,
        BrickState[] bricks,
        PowerUpState[] powerUps,
        ScorePopupState[] scorePopups,
        OverlayState overlay);

    private sealed record RectState(int x, int y, int width, int height);
    private sealed record PaddleState(int x, int y, int width, int height);
    private sealed record BallState(int x, int y, int radius);
    private sealed record BrickState(int x, int y, int width, int height, RgbColor color);
    private sealed record PowerUpState(int x, int y, int width, int height, string kind);
    private sealed record ScorePopupState(int x, int y, string text, float opacity, bool isMultiplier);
    private sealed record OverlayState(int score, int highScore, int level, double borderHue, bool isPaused, bool isGameOver, bool ballReady);
    private sealed record RgbColor(int r, int g, int b);
}
