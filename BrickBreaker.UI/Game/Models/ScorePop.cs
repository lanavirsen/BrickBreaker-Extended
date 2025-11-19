
namespace BrickBreaker.Game.Models
{
    public class ScorePop // Class representing a score popup in the game
    {
        public int X { get; set; } // Horizontal position of the score popup
        public int Y { get; set; } // Vertical position of the score popup
        public int Score { get; set; } // Score value to be displayed
        public int Duration { get; set; } // Duration for which the score popup is visible
    }
}