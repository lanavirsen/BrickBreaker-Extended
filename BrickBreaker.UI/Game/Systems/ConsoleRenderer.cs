using System;
using BrickBreaker.Game.Models;                    // Imports core game models (Ball, PowerUp, ScorePop, etc.)
using BrickBreaker.UI.Game.Models;                 // Imports UI/game-specific models (like PowerUpType)
using static BrickBreaker.Game.Models.Constants;   // Imports constants directly for easy access (W, H, TopMargin, etc.)

namespace BrickBreaker.UI.Game.Renderer            // Namespace for rendering-related classes
{
    // Handles rendering all game elements to the console window
    public class ConsoleRenderer
    {
        // Palette used to color each horizontal brick layer
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

        // Renders the entire game frame, including UI, bricks, paddle, balls, power-ups, and score pops
        public void Render(
            int lives,                          // Number of remaining lives
            int score,                          // Current score
            int currentLevel,                   // The current level (zero-indexed)
            int hitMultiplier,                  // Points multiplier for consecutive hits
            bool isPaused,                      // Game paused state
            bool[,] bricks,                     // 2D array indicating the presence of bricks
            int paddleX,                        // Leftmost X-coordinate of the paddle
            int paddleWidth,                    // Paddle width in characters/blocks
            List<Ball> balls,                   // List of all balls in play
            List<PowerUp> powerUps,             // List of all active power-ups on the field
            List<ScorePop> scorePops)           // List of visual score popups to display
        {
            const int paddleY = H - 2;          // Paddle is always at this Y row from the bottom

            Console.ResetColor();               // Clears any previous text color settings

            // Render player lives, score, and level at the top left
            Console.SetCursorPosition(2, 0);    // Move cursor to near top left
            Console.ForegroundColor = ConsoleColor.Green;   // Set color to green
            Console.Write($"Lives: {lives,2}  Score: {score,7}  Level: {currentLevel + 1,2}      "); // Print lives, score, and level
            Console.ResetColor();               // Reset to default color

            // Render hit multiplier at the top right
            Console.SetCursorPosition(W + 5, 0);          // Move cursor to the desired right offset
            Console.ForegroundColor = ConsoleColor.Yellow;// Set color to yellow
            Console.Write($"Multiplier: x{hitMultiplier,2}    "); // Print the hit multiplier
            Console.ResetColor();                         // Reset color again

            // Draw music control instructions
            Console.SetCursorPosition(W + 4, 4);          // Move cursor to under the score area
            Console.Write("Press 'N' for next track, 'P' to pause/resume music"); // Show music controls

            DrawGameBoard(bricks, paddleX, paddleWidth, paddleY, balls, powerUps, scorePops);

            // If the game is paused, show a "PAUSED" message in the upper-mid area
            if (isPaused)
            {
                Console.SetCursorPosition(W / 2 - 3, 1); // Move cursor to center-ish
                Console.ForegroundColor = ConsoleColor.Yellow; // Use yellow text
                Console.Write("PAUSED ");                // Show paused status
                Console.ResetColor();                    // Restore default color
            }
        }

        private void DrawGameBoard(
            bool[,] bricks,
            int paddleX,
            int paddleWidth,
            int paddleY,
            List<Ball> balls,
            List<PowerUp> powerUps,
            List<ScorePop> scorePops)
        {
            Console.SetCursorPosition(0, 1);
            Console.Write('┌');
            Console.Write(new string('─', W - 2));
            Console.Write('┐');

            int cols = bricks.GetLength(0);
            int rows = bricks.GetLength(1);
            int brickTop = TopMargin + 1;
            int brickBottom = brickTop + rows;

            for (int y = 1; y < H - 1; y++)
            {
                Console.SetCursorPosition(0, y + 1);
                Console.Write('│');

                ConsoleColor? currentColor = null;
                for (int x = 1; x < W - 1; x++)
                {
                    var (ch, color) = ResolveCell(
                        x, y, paddleX, paddleWidth, paddleY, bricks, cols, rows, brickTop, brickBottom,
                        balls, powerUps, scorePops);

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
                {
                    Console.ResetColor();
                }

                Console.Write('│');
            }

            Console.SetCursorPosition(0, H);
            Console.ResetColor();
            Console.Write('└');
            Console.Write(new string('─', W - 2));
            Console.Write('┘');
        }

        private static (char ch, ConsoleColor? color) ResolveCell(
            int x,
            int y,
            int paddleX,
            int paddleWidth,
            int paddleY,
            bool[,] bricks,
            int cols,
            int rows,
            int brickTop,
            int brickBottom,
            List<Ball> balls,
            List<PowerUp> powerUps,
            List<ScorePop> scorePops)
        {
            char ch = ' ';
            ConsoleColor? color = null;

            if (cols > 0 && rows > 0 && y >= brickTop && y < brickBottom)
            {
                int r = y - brickTop;
                int c = (x - 1) * cols / (W - 2);
                c = Math.Clamp(c, 0, cols - 1);
                if (bricks[c, r])
                {
                    ch = '█';
                    color = GetBrickColor(r, rows);
                }
            }

            if (y == paddleY && x >= paddleX && x < paddleX + paddleWidth)
            {
                ch = '█';
                color = null;
            }

            foreach (var ball in balls)
            {
                if (x == ball.X && y == ball.Y)
                {
                    ch = ball.IsMultiball ? '*' : '●';
                    color = null;
                }
            }

            foreach (var pu in powerUps)
            {
                if (x == pu.X && y == pu.Y)
                {
                    ch = pu.Type == PowerUpType.MultiBall ? 'M' : 'E';
                    color = null;
                }
            }

            foreach (var pop in scorePops)
            {
                string scoreText = $"+{pop.Score}";
                if (y == pop.Y && x >= pop.X && x < pop.X + scoreText.Length)
                {
                    ch = scoreText[x - pop.X];
                    color = null;
                }
            }

            return (ch, color);
        }

        private static ConsoleColor GetBrickColor(int row, int totalRows)
        {
            if (totalRows <= 1)
                return BrickLayerColors[0];

            double t = row / (double)(totalRows - 1);
            int index = (int)Math.Round(t * (BrickLayerColors.Length - 1));
            index = Math.Clamp(index, 0, BrickLayerColors.Length - 1);
            return BrickLayerColors[index];
        }
    }
}
