using BrickBreaker.Game.Entities;
using BrickBreaker.Game.Utilities;
using BrickBreaker.Gameplay;

namespace BrickBreaker.ConsoleClient.Game.Systems;

public sealed class ConsoleRenderer
{
    private const int ConsoleW = 62;
    private const int ConsoleH = 24;
    private const int InnerW = ConsoleW - 2; // 60 columns inside the border
    private const int InnerH = ConsoleH - 2; // 22 rows inside the border
    private const double PixelW = 622.0;
    private const double PixelH = 568.0;
    private const int PaddleHeightPx = 20; // Matches WinForms paddle draw height

    // Tracks the horizontal offset from the previous frame so a full clear can be
    // issued when a resize shifts the box, removing the ghost of the old position.
    private int _lastLeft = -1;

    private static readonly ConsoleColor[] BrickLayerColors =
    {
        ConsoleColor.DarkBlue,
        ConsoleColor.Blue,
        ConsoleColor.DarkCyan,
        ConsoleColor.Cyan,
        ConsoleColor.DarkGreen,
        ConsoleColor.Green,
        ConsoleColor.DarkYellow,
        ConsoleColor.Yellow
    };

    public void Render(GameRenderState state)
    {
        Console.ResetColor();

        // Centre the box horizontally if the terminal is wider than the game area.
        // Clamp to 0 so a narrow window falls back to left-aligned without throwing.
        int left = Math.Max(0, (GetWindowWidth() - ConsoleW) / 2);

        // When the offset changes the old frame is now at a different column.
        // A full clear removes the ghost before redrawing at the new position.
        if (left != _lastLeft)
        {
            Console.Clear();
            _lastLeft = left;
        }

        // HUD: three elements spread across row 0 — left, centre, right.
        string scoreText = $"Score: {state.Score,7}";
        string levelText = $"Level: {state.Level,2}";

        Console.ForegroundColor = ConsoleColor.Green;
        Console.SetCursorPosition(left + 2, 0);
        Console.Write($"Lives: {state.Balls.Count}");
        Console.SetCursorPosition(left + (ConsoleW - scoreText.Length) / 2, 0);
        Console.Write(scoreText);
        Console.SetCursorPosition(left + ConsoleW - levelText.Length - 2, 0);
        Console.Write(levelText);
        Console.ResetColor();

        DrawGameBoard(state, left);
    }

    private static void DrawGameBoard(GameRenderState state, int left)
    {
        Console.SetCursorPosition(left, 1);
        DrawTopBorder(state.IsPaused);

        for (int cy = 1; cy <= InnerH; cy++)
        {
            Console.SetCursorPosition(left, cy + 1);
            Console.Write('│');

            ConsoleColor? currentColor = null;
            for (int cx = 1; cx <= InnerW; cx++)
            {
                var (ch, color) = ResolveCell(cx, cy, state);

                if (currentColor != color)
                {
                    if (color.HasValue)
                        Console.ForegroundColor = color.Value;
                    else
                        Console.ResetColor();
                    currentColor = color;
                }

                Console.Write(ch);
            }

            if (currentColor.HasValue)
                Console.ResetColor();

            Console.Write('│');
        }

        Console.SetCursorPosition(left, ConsoleH);
        Console.ResetColor();
        Console.Write('└');
        Console.Write(new string('─', ConsoleW - 2));
        Console.Write('┘');
    }

    private static void DrawTopBorder(bool isPaused)
    {
        int innerWidth = ConsoleW - 2;
        Console.Write('┌');

        if (!isPaused)
        {
            Console.Write(new string('─', innerWidth));
        }
        else
        {
            const string message = " << PAUSED >> ";
            if (message.Length >= innerWidth)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(message[..innerWidth]);
                Console.ResetColor();
            }
            else
            {
                int leftCount = Math.Max(0, (innerWidth - message.Length) / 2);
                int rightCount = Math.Max(0, innerWidth - leftCount - message.Length);
                Console.Write(new string('─', leftCount));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(message);
                Console.ResetColor();
                Console.Write(new string('─', rightCount));
            }
        }

        Console.Write('┐');
    }

    // Maps a console cell (cx, cy) to the game element that should be drawn there,
    // in priority order: bricks → paddle → balls → power-ups → score popups.
    //
    // Bricks and the paddle use center-Y to select their row (preventing boundary
    // bleed when an object is nearly exactly one cell tall), then a horizontal
    // overlap check to spread across the correct number of columns.
    // Balls and power-ups are point-mapped to a single cell via their center.
    private static (char ch, ConsoleColor? color) ResolveCell(int cx, int cy, GameRenderState state)
    {
        double pxLeft = (cx - 1) * PixelW / InnerW;
        double pxRight = cx * PixelW / InnerW;

        char ch = ' ';
        ConsoleColor? color = null;

        // 1. Bricks — one row per brick (center-Y), full-width horizontal spread
        foreach (var brick in state.Bricks)
        {
            int brickCellY = 1 + (int)((brick.Y + brick.Height / 2.0) / PixelH * InnerH);
            if (brickCellY == cy &&
                (double)brick.X < pxRight && brick.X + brick.Width > pxLeft)
            {
                ch = '█';
                color = GetBrickColor(brick.Y);
                break;
            }
        }

        // 2. Paddle — one row (center-Y), full-width horizontal spread
        int paddleCellY = 1 + (int)((state.PaddleY + PaddleHeightPx / 2.0) / PixelH * InnerH);
        if (paddleCellY == cy &&
            (double)state.PaddleX < pxRight && state.PaddleX + state.PaddleWidth > pxLeft)
        {
            ch = '█';
            color = state.PaddleBlinking ? ConsoleColor.DarkRed : null;
        }

        // 3. Balls — center point → single cell
        foreach (var ball in state.Balls)
        {
            int ballCx = 1 + (int)((ball.X + ball.Radius) / PixelW * InnerW);
            int ballCy = 1 + (int)((ball.Y + ball.Radius) / PixelH * InnerH);
            if (ballCx == cx && ballCy == cy)
            {
                ch = '●';
                color = null;
            }
        }

        // 4. Power-ups — center point → single cell
        foreach (var pu in state.PowerUps)
        {
            int puCellX = 1 + (int)((pu.X + pu.Width / 2.0) / PixelW * InnerW);
            int puCellY = 1 + (int)((pu.Y + pu.Height / 2.0) / PixelH * InnerH);
            if (puCellX == cx && puCellY == cy)
            {
                ch = pu.Type == PowerUpType.Multiball ? 'M' : 'E';
                color = null;
            }
        }

        // 5. Score popups — render text starting at the mapped character position
        foreach (var popup in state.ScorePopups)
        {
            if (popup.Opacity < 0.1f) continue;

            int popCx = 1 + (int)(popup.X / PixelW * InnerW);
            int popCy = 1 + (int)(popup.Y / PixelH * InnerH);
            if (cy == popCy && cx >= popCx && cx < popCx + popup.Text.Length)
            {
                ch = popup.Text[cx - popCx];
                color = null;
            }
        }

        return (ch, color);
    }

    // Returns the console window width, or 0 when output is redirected or the
    // call fails, so the offset calculation safely falls back to left-aligned.
    private static int GetWindowWidth()
    {
        if (Console.IsOutputRedirected) return 0;
        try { return Console.WindowWidth; }
        catch { return 0; }
    }

    // Derives a console color from the brick's Y pixel position using the same
    // 8-color palette spread across the brick rows as the original renderer.
    private static ConsoleColor GetBrickColor(int brickY)
    {
        int row = (int)((brickY - GameConstants.PlayAreaMargin) / (double)GameConstants.BrickYSpacing);
        row = Math.Clamp(row, 0, GameConstants.InitialBrickRows - 1);
        double t = row / (double)(GameConstants.InitialBrickRows - 1);
        int index = (int)Math.Round(t * (BrickLayerColors.Length - 1));
        return BrickLayerColors[Math.Clamp(index, 0, BrickLayerColors.Length - 1)];
    }
}
