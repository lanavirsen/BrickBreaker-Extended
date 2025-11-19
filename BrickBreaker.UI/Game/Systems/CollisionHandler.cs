using BrickBreaker.Game.Models;                           // Imports game models (e.g., Ball, ScorePop)
using BrickBreaker.UI.Game.Models;                        // Imports UI/game-specific models
using static BrickBreaker.Game.Models.Constants;           // Imports constants for direct use (e.g., W, H, TopMargin)

namespace BrickBreaker.Game.Systems                       // Defines the namespace for organizational structure
{
    // BrickHitInfo class indicates information about a brick being hit, inherits from ScorePop
    public class BrickHitInfo : ScorePop
    {
        public int BrickCol { get; set; }                 // Column index of the brick hit
        public int BrickRow { get; set; }                 // Row index of the brick hit
    }

    // Handles collision detection and response logic
    public class CollisionHandler
    {
        // Applies bounce logic when the ball hits the paddle; accepts the paddle's width
        public void ApplyPaddleBounce(Ball ball, int hitPos, int paddleWidth)
        {
            double halfWidth = (paddleWidth - 1) / 2.0;                        // Calculates half the width of the paddle
            double normalizedOffset = (hitPos - halfWidth) / halfWidth;        // Normalizes the hit position to [-1, 1]

            const double maxSpeed = 2.4;                                       // Maximum ball horizontal speed
            double shapedSpeed = Math.Sign(normalizedOffset) *                 // Adjusts speed shape for better control/feel
                Math.Pow(Math.Abs(normalizedOffset), 0.65);

            double newVx = Math.Clamp(shapedSpeed * maxSpeed, -maxSpeed, maxSpeed); // Clamps velocity between allowed extremes

            const double minAbsSpeed = 0.25;                                   // Minimum absolute horizontal speed
            if (Math.Abs(newVx) < minAbsSpeed)                                // Ensures speed isn't too low
                newVx = (ball.X < W / 2) ? -minAbsSpeed : minAbsSpeed;        // Guarantees direction away from center

            ball.SetHorizontalVelocity(newVx, 0);                              // Sets the new horizontal velocity
        }

        // Checks if a brick exists at the provided pixel coordinates (x, y)
        public (bool hit, int c, int r) BrickAt(bool[,] bricks, int x, int y)
        {
            int cols = bricks.GetLength(0), rows = bricks.GetLength(1);        // Gets the grid size for bricks
            int brickTop = TopMargin + 1, brickBottom = TopMargin + 1 + rows;  // Defines vertical brick grid range

            if (y < brickTop || y >= brickBottom) return (false, -1, -1);      // Outside brick area, no hit

            int r = y - brickTop;                                              // Calculates brick row
            int c = (x - 1) * cols / (W - 2);                                  // Scales x to the correct column based on width
            c = Math.Clamp(c, 0, cols - 1);                                    // Clamps column to valid range

            return (bricks[c, r], c, r);                                       // Returns hit status and position
        }

        // Handles all logic for updating a ball's position, including collision handling
        public (bool isRemoved, bool brickHit) UpdateBall(
            Ball ball,
            bool[,] bricks,
            int paddleX,
            int paddleY,
            int paddleWidth, // Receives current paddle width
            out BrickHitInfo? hitInfo)
        {
            hitInfo = null;                                                    // Initializes brick hit info
            bool brickHit = false;                                             // Initializes flag for brick hit

            // -- Horizontal movement calculation --
            ball.UpdateAndGetDx(out int dxStep);                               // Updates the ball's X velocity & gets step value
            int nx = ball.X + dxStep;                                          // Calculates new X position
            int ny = ball.Y + ball.Dy;                                         // Calculates new Y position

            // -- Wall collisions --
            if (nx <= 1)                                                       // Left wall
            {
                ball.InvertHorizontalVelocity();                               // Bounce left
                nx = 2;                                                        // Keep inside left edge
            }
            else if (nx >= W - 2)                                              // Right wall
            {
                ball.InvertHorizontalVelocity();                               // Bounce right
                nx = W - 3;                                                    // Keep inside right edge
            }

            if (ny <= TopMargin)                                               // Top wall
            {
                ball.InvertVerticalVelocity();                                 // Bounce upwards
                ny = TopMargin + 1;                                            // Keep inside top edge
            }

            // -- Paddle collision --
            if (ball.Dy > 0 && ny >= paddleY && nx >= paddleX && nx < paddleX + paddleWidth)
            {
                ball.InvertVerticalVelocity();                                 // Bounce downwards
                int hitPos = Math.Clamp(nx - paddleX, 0, paddleWidth - 1);     // Normalizes hit position on paddle
                ApplyPaddleBounce(ball, hitPos, paddleWidth);                  // Applies curved bounce logic

                ny = paddleY - 1;                                              // Place above paddle
                ball.SetPosition(ball.X, ny);                                  // Update Y position, keep X
                return (false, false);                                         // Ball continues, no brick hit
            }

            // -- Check brick collision along X-axis only --
            bool brickHitX = false;                                            // Tracks if an X-axis brick collision occurs
            if (nx != ball.X)
            {
                var (hitX, cx, rx) = BrickAt(bricks, nx, ball.Y);              // Tests for collision in X movement
                if (hitX)
                {
                    hitInfo = new BrickHitInfo { BrickCol = cx, BrickRow = rx, Duration = 30 }; // Stores brick info
                    ball.InvertHorizontalVelocity();                           // Bounce left/right
                    nx = ball.X;                                               // Prevent horizontal over-stepping
                    brickHitX = true;
                    brickHit = true;
                }
            }

            // -- Check brick collision along Y-axis (potentially after X) --
            // Uses potentially modified X position (nx)
            if (ny != ball.Y)
            {
                var (hitY, cy, ry) = BrickAt(bricks, nx, ny);                  // Tests for collision in Y movement
                if (hitY)
                {
                    if (!brickHitX) // Only create new hitInfo if no collision on X in this frame
                    {
                        hitInfo = new BrickHitInfo { BrickCol = cy, BrickRow = ry, Duration = 30 };
                    }
                    ball.InvertVerticalVelocity();                             // Bounce up/down
                    ny = ball.Y;                                               // Prevent vertical over-stepping
                    brickHit = true;
                }
            }

            // -- Update the ball's position after all collisions --
            ball.SetPosition(nx, ny);

            // -- Check if the ball has gone out-of-bounds (fell below screen) --
            if (ball.Y >= H - 1)
                return (true, brickHit);                                       // Indicates ball is lost/removed from play

            return (false, brickHit);                                          // Ball stays in, returns if brick was hit
        }
    }
}
