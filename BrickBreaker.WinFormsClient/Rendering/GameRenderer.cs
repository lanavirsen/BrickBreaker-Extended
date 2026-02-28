
using System.Drawing.Text;
using BrickBreaker.Game.Entities;
using BrickBreaker.Gameplay;

namespace BrickBreaker.WinFormsClient.Rendering;

public sealed class GameRenderer
{
    private readonly Color _background = Color.FromArgb(8, 6, 20);
    private readonly Color _playAreaFill = Color.FromArgb(22, 22, 40);
    private readonly Color _playAreaBorder = Color.FromArgb(84, 66, 140);

    // Entry point called every frame. Draws all layers in painter's order
    // (background first, paddle last so it always appears on top).
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

    // Draws the bordered play area rectangle. The border is drawn first so the
    // fill covers its inner half, leaving a clean outer edge.
    private void DrawPlayArea(Graphics g, GameRenderState state)
    {
        using Pen borderPen = new(_playAreaBorder, 6);
        g.DrawRectangle(borderPen, state.PlayArea);
        using SolidBrush areaBrush = new(_playAreaFill);
        g.FillRectangle(areaBrush, state.PlayArea);
    }

    // Each brick is filled with its own colour then outlined in black.
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

    // All balls share a single brush since they're the same colour.
    private void DrawBalls(Graphics g, GameRenderState state)
    {
        using SolidBrush fill = new(Color.FromArgb(111, 224, 255)); // light teal to match canvas palette
        foreach (var ball in state.Balls)
        {
            Rectangle rect = new(ball.X, ball.Y, ball.Radius * 2, ball.Radius * 2);
            g.FillEllipse(fill, rect);
        }
    }

    // Power-ups are drawn as coloured circles with a letter indicating their type:
    // M = Multiball, E = Extender.
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

    // Score popups fade out as their opacity drops. A 2px shadow is drawn first
    // for legibility against varied brick colours.
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

    // Draws the title centred above the play area, plus score and level stat boxes
    // aligned to the left and right edges of the play area respectively.
    private void DrawHud(Graphics g, GameRenderState state, FontResources fonts, Rectangle clientRect)
    {
        var playArea = state.PlayArea;
        int hudY = playArea.Top - 40;

        // Title — shadow offset by 4px gives a cheap depth effect.
        string title = "BRICK BREAKER";
        SizeF titleSize = g.MeasureString(title, fonts.GameTitle);
        float titleX = (clientRect.Width - titleSize.Width) / 2;
        float titleY = Math.Max(playArea.Top - 90, 10);
        g.DrawString(title, fonts.GameTitle, Brushes.Gray, titleX + 4, titleY + 4);
        g.DrawString(title, fonts.GameTitle, Brushes.Cyan, titleX, titleY);

        // Level box is right-aligned to the play area using measured text width.
        string levelText = $"Level {state.Level}";
        SizeF levelSize = g.MeasureString(levelText, fonts.Level);
        int levelBoxW = (int)levelSize.Width + 20; // 10px padding each side
        DrawStatBox(g, $"Score: {state.Score}", fonts.Score, playArea.Left, hudY);
        DrawStatBox(g, levelText, fonts.Level, playArea.Right - levelBoxW, hudY);
    }

    // Dark-filled box sized to its text content, with uniform padding on all sides.
    private void DrawStatBox(Graphics g, string text, Font font, int x, int y)
    {
        SizeF size = g.MeasureString(text, font);
        int padding = 10;
        Rectangle rect = new(x, y, (int)size.Width + padding * 2, (int)size.Height + padding * 2);
        using SolidBrush background = new(Color.FromArgb(0, 0, 20));
        g.FillRectangle(background, rect);
        g.DrawString(text, font, Brushes.White, x + padding, y + padding);
    }

    // Full-screen overlays shown when the game state changes. Only one is ever
    // active at a time: game over takes priority, then launch prompt, then pause.
    private void DrawOverlays(Graphics g, GameRenderState state, FontResources fonts, Rectangle clientRect)
    {
        if (state.IsGameOver)
        {
            using SolidBrush dim = new(Color.FromArgb(150, 0, 0, 0));
            g.FillRectangle(dim, clientRect);
            string txt = "Game Over! Press ENTER to restart";
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

    // Paddle blinks orange when a power-up expiry is imminent; otherwise uses
    // the colour passed in from the form.
    private void DrawPaddle(Graphics g, GameRenderState state, Color paddleColor)
    {
        Rectangle rect = new((int)state.PaddleX, state.PaddleY, state.PaddleWidth, 20);
        Color color = state.PaddleBlinking && (DateTime.Now.Millisecond / 100) % 2 == 0
            ? Color.OrangeRed
            : paddleColor;

        using SolidBrush brush = new(color);
        g.FillRectangle(brush, rect);
    }
}

// Font set passed to the renderer each frame. Owned and disposed by Form1.
public sealed record FontResources(
    Font Score,
    Font Multiplier,
    Font Level,
    Font Launch,
    Font GameOver,
    Font GameTitle);
