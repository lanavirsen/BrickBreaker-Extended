using BrickBreaker.Game.Models;                    // Imports core game models (Ball, PowerUp, ScorePop, etc.)
using BrickBreaker.UI.Game.Models;                 // Imports UI/game-specific models (like PowerUpType)
using System.Text;                                 // Imports functionality for working with strings efficiently
using static BrickBreaker.Game.Models.Constants;   // Imports constants directly for easy access (W, H, TopMargin, etc.)

namespace BrickBreaker.UI.Game.Renderer            // Namespace for rendering-related classes
{
    // Handles rendering all game elements to the console window
    public class ConsoleRenderer
    {
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

            // -- Start drawing the main game board into a string buffer --
            var sb = new StringBuilder((W + 1) * (H + 2));                 // Buffer to build the frame to print

            sb.Append('┌'); sb.Append('─', W - 2); sb.Append('┐').Append('\n'); // Draw top border

            for (int y = 1; y < H - 1; y++)           // Outer loop for each row (excluding borders)
            {
                sb.Append('│');                       // Left wall

                for (int x = 1; x < W - 1; x++)       // Inner loop for each column (excluding borders)
                {
                    char ch = ' ';                    // Default: empty space

                    int cols = bricks.GetLength(0), rows = bricks.GetLength(1);       // Get brick grid size
                    int brickTop = TopMargin + 1, brickBottom = TopMargin + 1 + rows; // Compute brick grid area

                    // If within brick region, see if a brick is present
                    if (y >= brickTop && y < brickBottom)
                    {
                        int r = y - brickTop;                                         // Convert y to brick row index
                        int c = (x - 1) * cols / (W - 2);                             // Convert x to brick column index
                        if (bricks[c, r]) ch = '█';                                   // If there's a brick, draw it
                    }
                    // Check paddle: draw solid paddle if within the correct row and x-range
                    if (y == paddleY && x >= paddleX && x < paddleX + paddleWidth) ch = '█';

                    // Draw all balls: either an asterisk for multiball, or a circle for regular ball
                    foreach (var ball in balls)
                    {
                        if (x == ball.X && y == ball.Y)
                            ch = ball.IsMultiball ? '*' : '●';
                    }

                    // Draw all power-ups, different character for each type
                    foreach (var pu in powerUps)
                    {
                        if (x == pu.X && y == pu.Y)
                        {
                            // 'M' for MultiBall powerup, 'E' for Expand Paddle
                            ch = pu.Type == PowerUpType.MultiBall ? 'M' : 'E';
                        }
                    }
                    // Draw all score popups as number text, positionally mapped
                    foreach (var pop in scorePops)
                    {
                        string scoreText = $"+{pop.Score}";          // Make the score popup string
                        // If the current position matches the popup area, draw the appropriate character from the popup text
                        if (y == pop.Y && x >= pop.X && x < pop.X + scoreText.Length)
                        {
                            ch = scoreText[x - pop.X];
                        }
                    }

                    sb.Append(ch);                       // Append chosen character to this cell in frame
                }
                sb.Append('│').Append('\n');             // Right wall and next row
            }
            sb.Append('└'); sb.Append('─', W - 2); sb.Append('┘');         // Draw bottom border

            // -- End board drawing, output the buffer to console --

            Console.SetCursorPosition(0, 1);             // Move cursor below UI rows
            Console.Write(sb.ToString());                // Print the entire game area at once

            // If the game is paused, show a "PAUSED" message in the upper-mid area
            if (isPaused)
            {
                Console.SetCursorPosition(W / 2 - 3, 1); // Move cursor to center-ish
                Console.ForegroundColor = ConsoleColor.Yellow; // Use yellow text
                Console.Write("PAUSED ");                // Show paused status
                Console.ResetColor();                    // Restore default color
            }
        }
    }
}
