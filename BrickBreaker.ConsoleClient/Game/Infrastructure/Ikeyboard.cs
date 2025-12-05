namespace BrickBreaker.ConsoleClient.Game.Infrastructure
{
    public interface IKeyboard // Interface for keyboard input handling
    {
        bool IsLeftPressed(); // Checks if the left key is pressed
        bool IsRightPressed(); // Checks if the right key is pressed
        bool IsEscapePressed(); // Checks if the escape key is pressed
        bool IsSpacePressed(); // Checks if the space key is pressed
        bool IsUpPressed(); // Checks if the up key is pressed
    }

}
