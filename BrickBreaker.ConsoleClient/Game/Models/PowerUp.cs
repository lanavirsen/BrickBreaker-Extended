namespace BrickBreaker.ConsoleClient.Game.Models
{
    public enum PowerUpType // Enumeration for different types of power-ups
    {
        MultiBall, // Existing power-up type
        PaddleExpand // New power-up type for expanding the paddle
    }

    public class PowerUp // Class representing a power-up in the game
    {
        public int X, Y; // Position of the power-up
        public PowerUpType Type; // Type of the power-up
        public PowerUp(int x, int y, PowerUpType type) // Constructor to initialize power-up properties
        {
            X = x; // Set horizontal position
            Y = y; // Set vertical position
            Type = type; // Set power-up type
        }
    }
}