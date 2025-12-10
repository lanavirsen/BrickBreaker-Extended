using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BrickBreaker.Game;
using BrickBreaker.Game.Entities;
using BrickBreaker.Game.Utilities;

namespace BrickBreaker.Gameplay;

public sealed class GameSession
{
    private readonly GameEngine _engine = new();
    private Rectangle _playArea;
    private double _paddleX;
    private int _paddleY;
    private bool _leftPressed;
    private bool _rightPressed;
    private bool _ballReady = true;
    private bool _isPaused;
    private bool _isGameOver;
    private double _elapsedSeconds;
    private float _borderHue;
    private double _lastPaddleWidth; // Track changes so extender grows/shrinks symmetrically

    public event EventHandler<int>? GameFinished;

    public GameRenderState Snapshot { get; private set; } = GameRenderState.Empty;

    public GameSession()
    {
        _engine.GameOver += (_, _) =>
        {
            _isGameOver = true;
            _ballReady = true;
            GameFinished?.Invoke(this, _engine.Score);
        };

        _engine.LevelLoaded += (_, _) =>
        {
            _ballReady = true;
            SnapBallToPaddle();
        };
    }

    public void Initialize(Rectangle playArea)
    {
        _playArea = playArea;
        ResetState();
        _engine.StartLevel(1, _playArea);
        RefreshSnapshot();
    }

    public void UpdatePlayArea(Rectangle newArea)
    {
        if (_playArea == Rectangle.Empty)
        {
            Initialize(newArea);
            return;
        }

        int dx = newArea.X - _playArea.X;
        int dy = newArea.Y - _playArea.Y;
        _engine.ShiftWorld(dx, dy);
        _paddleX += dx;
        _paddleY += dy;
        _playArea = newArea;
        ClampPaddleWithinBounds();
        RefreshSnapshot();
    }

    public void Update(double deltaTime)
    {
        if (_isGameOver || _isPaused)
        {
            UpdateBorder(deltaTime);
            RefreshSnapshot();
            return;
        }

        UpdatePaddleMovement();
        UpdateBorder(deltaTime);

        if (_ballReady)
        {
            SnapBallToPaddle();
        }
        else
        {
            _elapsedSeconds += deltaTime;
            _engine.Update(deltaTime, _playArea, _paddleX, _paddleY);
            SyncPaddleWidth(); // recenters paddle when extender grows/shrinks it
        }

        RefreshSnapshot();
    }

    public void SetInput(bool leftPressed, bool rightPressed)
    {
        _leftPressed = leftPressed;
        _rightPressed = rightPressed;
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
    }

    public void Restart()
    {
        ResetState();
        _engine.StartLevel(1, _playArea);
        RefreshSnapshot();
    }

    private void ResetState()
    {
        _paddleY = _playArea.Bottom - 40;
        _paddleX = _playArea.Left + (_playArea.Width - _engine.CurrentPaddleWidth) / 2;
        _elapsedSeconds = 0;
        _borderHue = 0;
        _ballReady = true;
        _isPaused = false;
        _isGameOver = false;
        _lastPaddleWidth = _engine.CurrentPaddleWidth;
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

        ClampPaddleWithinBounds();
    }

    private void UpdateBorder(double deltaTime)
    {
        _borderHue = (_borderHue + 120f * (float)deltaTime) % 360f;
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

    private void SyncPaddleWidth()
    {
        var currentWidth = _engine.CurrentPaddleWidth;
        if (Math.Abs(currentWidth - _lastPaddleWidth) < double.Epsilon)
        {
            return;
        }

        // Shift position by half the delta so width changes radiate from the center
        _paddleX -= (currentWidth - _lastPaddleWidth) / 2.0;
        _lastPaddleWidth = currentWidth;
        ClampPaddleWithinBounds();
    }

    // Ensure paddle stays centered when width changes and never leaves the play area
    private void ClampPaddleWithinBounds()
    {
        if (_playArea == Rectangle.Empty)
        {
            return;
        }

        if (_paddleX < _playArea.Left)
        {
            _paddleX = _playArea.Left;
        }

        var maxX = _playArea.Right - _engine.CurrentPaddleWidth;
        if (_paddleX > maxX)
        {
            _paddleX = maxX;
        }
    }

    private void RefreshSnapshot()
    {
        var bricks = _engine.Bricks
            .Where(b => b.IsVisible)
            .Select(b => new BrickRenderModel(b.X, b.Y, b.Width, b.Height, b.BrickColor))
            .ToArray();

        var balls = _engine.Balls
            .Select(b => new BallRenderModel(b.X, b.Y, b.Radius))
            .ToArray();

        var powerUps = _engine.PowerUps
            .Select(p => new PowerUpRenderModel(p.X, p.Y, p.Width, p.Height, p.Type))
            .ToArray();

        var popups = _engine.ScorePopups
            .Select(p => new ScorePopupRenderModel(p.X, p.Y, p.DisplayText, p.Opacity, p.IsMultiplier))
            .ToArray();

        Snapshot = new GameRenderState(
            _playArea,
            bricks,
            balls,
            powerUps,
            popups,
            _paddleX,
            _paddleY,
            _engine.CurrentPaddleWidth,
            _ballReady,
            _isPaused,
            _isGameOver,
            _borderHue,
            _elapsedSeconds,
            _engine.Score,
            _engine.CurrentLevel,
            _engine.HighScore,
            _engine.IsPaddleBlinking);
    }

}
