using BrickBreaker.UI.Game.Models;                 // Imports game models used by power-ups (like PowerUp, PowerUpType, Ball)

namespace BrickBreaker.Game.Systems                // Namespace for system/game logic classes
{
    // Contains logic for how power-ups affect gameplay; methods are static for ease of use
    public static class PowerUpLogic
    {
        // Activates the behavior of the given power-up (multi-ball or paddle expand)
        public static void ActivatePowerUp(
            PowerUp pu,                          // The power-up being collected
            List<Ball> balls,                    // Reference to the current list of balls in play
            ref int paddleX,                     // Reference to the paddle's leftmost X-position (can be modified)
            ref int paddleWidth,                 // Reference to the paddle's width (can be modified)
            ref int paddleExtendTimer,           // Reference to the paddle's expansion timer (set/extended)
            int paddleY)                         // The Y-position of the paddle (usually fixed)
        {
            // If the powerup is MultiBall, spawn additional balls
            if (pu.Type == PowerUpType.MultiBall)
            {
                int paddleCenter = paddleX + paddleWidth / 2;         // Find the center X of the paddle

                // Create Ball 1: will go up and to the right
                var ball1 = new Ball(paddleCenter, paddleY - 1, 0, 0, true);
                ball1.SetHorizontalVelocity(1, 0);                    // Set initial X velocity to right
                ball1.SetVerticalVelocity(-1);                        // Set initial Y velocity upward

                // Create Ball 2: will go up and to the left
                var ball2 = new Ball(paddleCenter, paddleY - 1, 0, 0, true);
                ball2.SetHorizontalVelocity(-1, 0);                   // Set initial X velocity to left
                ball2.SetVerticalVelocity(-1);                        // Set initial Y velocity upward

                // Add both new balls to the game
                balls.Add(ball1);
                balls.Add(ball2);
            }
            // If the powerup is PaddleExpand, make the paddle bigger and reset extend timer
            else if (pu.Type == PowerUpType.PaddleExpand)
            {
                const int newWidth = 15;                              // Set the width to this value when expanded
                const int durationInFrames = 600;                     // Expansion lasts this many game frames (e.g., 10s at 60fps)

                if (paddleWidth < newWidth)                           // Only expand if not already at or above max wideness
                {
                    int widthDifference = newWidth - paddleWidth;     // Calculate how much to expand
                    paddleX -= widthDifference / 2;                   // Shift paddle left so the center stays the same
                    paddleWidth = newWidth;                           // Set the actual width

                    if (paddleX < 1) paddleX = 1;                     // Clamp to keep the paddle on-screen
                }
                paddleExtendTimer = durationInFrames;                 // Start or reset the timer for shrink-back
            }
        }
    }
}
