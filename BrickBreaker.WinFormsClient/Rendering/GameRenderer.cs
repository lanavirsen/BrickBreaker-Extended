using System;
using System.Drawing;
using System.Drawing.Text;
using BrickBreaker.Game.Entities;
using BrickBreaker.Game.Utilities;
using BrickBreaker.Gameplay;

namespace BrickBreaker.WinFormsClient.Rendering;

public sealed class GameRenderer
{
    private readonly Color _background = Color.Black;
    private readonly Color _playAreaFill = Color.FromArgb(22, 22, 40);

    public void Draw(Graphics g, GameRenderState state, FontResources fonts, Rectangle clientRect, Color paddleColor)
    {
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        g.Clear(_background);

        DrawPlayArea(g, state);
        DrawBricks(g, state);
        DrawBalls(g, state);
        DrawPowerUps(g, state, fonts.Launch);
        DrawScorePopups(g, state, fonts.Multiplier);
        DrawHud(g, state, fonts, clientRect);
        DrawOverlays(g, state, fonts, clientRect);
        DrawPaddle(g, state, paddleColor);
    }

    private void DrawPlayArea(Graphics g, GameRenderState state)
    {
        using Pen borderPen = new(ColorFromHSV(state.BorderHue, 1.0f, 1.0f), 12);
        g.DrawRectangle(borderPen, state.PlayArea);
        using SolidBrush areaBrush = new(_playAreaFill);
        g.FillRectangle(areaBrush, state.PlayArea);
    }

    private void DrawBricks(Graphics g, GameRenderState state)
    {
        using Pen outline = new(Color.Black, 2);
        foreach (var brick in state.Bricks)
        {
            Rectangle rect = new(brick.X, brick.Y, brick.Width, brick.Height);
            using SolidBrush brush = new(brick.Color);
            g.FillRectangle(brush, rect);
            g.DrawRectangle(outline, rect);
        }
    }

    private void DrawBalls(Graphics g, GameRenderState state)
    {
        using SolidBrush fill = new(Color.FromArgb(111, 224, 255)); // light teal to match canvas palette
        foreach (var ball in state.Balls)
        {
            Rectangle rect = new(ball.X, ball.Y, ball.Radius * 2, ball.Radius * 2);
            g.FillEllipse(fill, rect);
        }
    }

    private void DrawPowerUps(Graphics g, GameRenderState state, Font font)
    {
        foreach (var powerUp in state.PowerUps)
        {
            Rectangle rect = new(powerUp.X, powerUp.Y, powerUp.Width, powerUp.Height);
            Brush fill = powerUp.Type switch
            {
                PowerUpType.Multiball => Brushes.Yellow,
                PowerUpType.PaddleExtender => Brushes.Cyan,
                _ => Brushes.White
            };

            g.FillEllipse(fill, rect);
            g.DrawEllipse(Pens.Black, rect);

            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            string letter = powerUp.Type == PowerUpType.Multiball ? "M" : "E";
            g.DrawString(letter, font, Brushes.Black, rect, format);
        }
    }

    private void DrawScorePopups(Graphics g, GameRenderState state, Font font)
    {
        foreach (var popup in state.ScorePopups)
        {
            int alpha = (int)(255 * popup.Opacity);
            using SolidBrush shadow = new(Color.FromArgb(alpha, Color.Black));
            Color mainColor = popup.IsMultiplier
                ? Color.FromArgb(alpha, Color.OrangeRed)
                : Color.FromArgb(alpha, Color.Yellow);
            using SolidBrush main = new(mainColor);
            g.DrawString(popup.Text, font, shadow, popup.X + 2, popup.Y + 2);
            g.DrawString(popup.Text, font, main, popup.X, popup.Y);
        }
    }

    private void DrawHud(Graphics g, GameRenderState state, FontResources fonts, Rectangle clientRect)
    {
        var playArea = state.PlayArea;
        int hudY = playArea.Top - GameConstants.HudHeightOffset;
        string title = "BRICK BREAKER";
        SizeF titleSize = g.MeasureString(title, fonts.GameTitle);
        float titleX = (clientRect.Width - titleSize.Width) / 2;
        float titleY = Math.Max(playArea.Top - 100, 10);
        g.DrawString(title, fonts.GameTitle, Brushes.Gray, titleX + 4, titleY + 4);
        g.DrawString(title, fonts.GameTitle, Brushes.Cyan, titleX, titleY);

        string timeStr = $"{(int)state.ElapsedSeconds / 60:D2}:{(int)state.ElapsedSeconds % 60:D2}";
        DrawStatBox(g, $"Score: {state.Score}", fonts.Score, playArea.Left, hudY);
        DrawStatBox(g, timeStr, fonts.Time, playArea.Right - 110, hudY);

        string level = $"Level {state.Level}";
        g.DrawString(level, fonts.Level, Brushes.White, playArea.Left + 300, hudY + 6);
    }

    private void DrawStatBox(Graphics g, string text, Font font, int x, int y)
    {
        SizeF size = g.MeasureString(text, font);
        int padding = 10;
        Rectangle rect = new(x, y, (int)size.Width + padding * 2, (int)size.Height + padding * 2);
        using SolidBrush background = new(Color.FromArgb(0, 0, 20));
        using Pen border = new(Color.LightGray, 2);
        g.FillRectangle(background, rect);
        g.DrawRectangle(border, rect);
        g.DrawString(text, font, Brushes.White, x + padding, y + padding);
    }

    private void DrawOverlays(Graphics g, GameRenderState state, FontResources fonts, Rectangle clientRect)
    {
        if (state.IsGameOver)
        {
            using SolidBrush dim = new(Color.FromArgb(150, 0, 0, 0));
            g.FillRectangle(dim, clientRect);
            string txt = "Game Over! Press SPACE to restart";
            SizeF size = g.MeasureString(txt, fonts.GameOver);
            g.DrawString(txt, fonts.GameOver, Brushes.Red,
                (clientRect.Width - size.Width) / 2,
                (clientRect.Height - size.Height) / 2);
        }
        else if (state.BallReady && !state.IsPaused)
        {
            string txt = "Press UP ARROW to launch";
            SizeF size = g.MeasureString(txt, fonts.Launch);
            g.DrawString(txt, fonts.Launch, Brushes.White,
                (clientRect.Width - size.Width) / 2,
                state.PaddleY - 80);
        }
        else if (state.IsPaused)
        {
            string txt = "Paused";
            SizeF size = g.MeasureString(txt, fonts.GameOver);
            g.DrawString(txt, fonts.GameOver, Brushes.Yellow,
                (clientRect.Width - size.Width) / 2,
                (clientRect.Height - size.Height) / 2);
        }
    }

    private void DrawPaddle(Graphics g, GameRenderState state, Color paddleColor)
    {
        Rectangle rect = new((int)state.PaddleX, state.PaddleY, state.PaddleWidth, 20);
        Color color = state.PaddleBlinking && (DateTime.Now.Millisecond / 100) % 2 == 0
            ? Color.OrangeRed
            : paddleColor;

        using SolidBrush brush = new(color);
        using Pen outline = new(Color.Blue, 2);
        g.FillRectangle(brush, rect);
        g.DrawRectangle(outline, rect);
    }

    private static Color ColorFromHSV(float hue, float saturation, float value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60.0 - Math.Floor(hue / 60.0);
        value *= 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => Color.FromArgb(255, v, t, p),
            1 => Color.FromArgb(255, q, v, p),
            2 => Color.FromArgb(255, p, v, t),
            3 => Color.FromArgb(255, p, q, v),
            4 => Color.FromArgb(255, t, p, v),
            _ => Color.FromArgb(255, v, p, q)
        };
    }
}

public sealed record FontResources(
    Font Score,
    Font Multiplier,
    Font Level,
    Font Time,
    Font Launch,
    Font GameOver,
    Font GameTitle);
