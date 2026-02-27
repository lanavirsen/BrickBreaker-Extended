namespace BrickBreaker.Game.Utilities
{
    public static class GameConstants
    {
        // Window Layout
        public const int PlayAreaMargin = 2;
        public const int HudHeightOffset = 50;
        public const int PaddleBottomMargin = 10;

        // Gameplay Constants
        public const int InitialBrickRows = 7;
        public const int InitialBrickCols = 10;
        public const int BrickWidth = 60;
        public const int BrickHeight = 25;
        // Keep only a thin gap between bricks so the wall looks tighter on both web and desktop clients.
        public const int BrickXSpacing = BrickWidth + 2;
        public const int BrickYSpacing = BrickHeight + 2;
        public const int PaddleAreaHeight = 381;

        // Physics
        public const double BasePaddleSpeed = 13;
        public const int BallRadius = 7;
    }
}
