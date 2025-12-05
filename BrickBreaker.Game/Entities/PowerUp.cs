namespace BrickBreaker.Game.Entities
{
    public enum PowerUpType
    {
        Multiball,
        PaddleExtender
    }

    public class PowerUp
    {
        public int X, Y;
        public PowerUpType Type;
        public int Width = 30;  // Consistent size for Hitbox and Drawing
        public int Height = 30;

        public PowerUp(int x, int y, PowerUpType type)
        {
            X = x;
            Y = y;
            Type = type;
        }

        public void UpdatePosition()
        {
            Y += 3; // Fall speed of the PowerUp
        }
    }
}
