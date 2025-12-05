using System;
using System.Collections.Generic;
using System.Drawing;
using BrickBreaker.Game.Entities;

namespace BrickBreaker.WinFormsClient.Gameplay;

public sealed record GameRenderState(
    Rectangle PlayArea,
    IReadOnlyList<BrickRenderModel> Bricks,
    IReadOnlyList<BallRenderModel> Balls,
    IReadOnlyList<PowerUpRenderModel> PowerUps,
    IReadOnlyList<ScorePopupRenderModel> ScorePopups,
    double PaddleX,
    int PaddleY,
    int PaddleWidth,
    bool BallReady,
    bool IsPaused,
    bool IsGameOver,
    float BorderHue,
    double ElapsedSeconds,
    int Score,
    int Level,
    bool PaddleBlinking)
{
    public static GameRenderState Empty { get; } = new(
        Rectangle.Empty,
        Array.Empty<BrickRenderModel>(),
        Array.Empty<BallRenderModel>(),
        Array.Empty<PowerUpRenderModel>(),
        Array.Empty<ScorePopupRenderModel>(),
        0,
        0,
        100,
        true,
        false,
        false,
        0f,
        0,
        0,
        1,
        false);
}

public sealed record BrickRenderModel(int X, int Y, int Width, int Height, Color Color);
public sealed record BallRenderModel(int X, int Y, int Radius);
public sealed record PowerUpRenderModel(int X, int Y, int Width, int Height, PowerUpType Type);
public sealed record ScorePopupRenderModel(int X, int Y, string Text, float Opacity, bool IsMultiplier);
