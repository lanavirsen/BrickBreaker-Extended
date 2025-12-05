using BrickBreaker.ConsoleClient.Game.Infrastructure;
using System.Runtime.InteropServices;

public sealed class Win32Keyboard : IKeyboard // Implementation of keyboard input handling using Win32 API
{
    [DllImport("user32.dll")] // Importing the GetAsyncKeyState function from user32.dll
    private static extern short GetAsyncKeyState(int vKey); // Retrieves the state of a key

    private static bool IsDown(int key) => (GetAsyncKeyState(key) & 0x8000) != 0; // Checks if a specific key is currently pressed

    private const int VK_LEFT = 0x25; // Virtual key code for the left arrow key
    private const int VK_RIGHT = 0x27; // Virtual key code for the right arrow key
    private const int VK_ESCAPE = 0x1B; // Virtual key code for the escape key

    public bool IsLeftPressed() => IsDown(VK_LEFT); // Checks if the left arrow key is pressed
    public bool IsRightPressed() => IsDown(VK_RIGHT); // Checks if the right arrow key is pressed
    public bool IsEscapePressed() => IsDown(VK_ESCAPE); // Checks if the escape key is pressed
    public bool IsSpacePressed() => IsDown((int)ConsoleKey.Spacebar); // Checks if the spacebar is pressed
    public bool IsUpPressed() => IsDown((int)ConsoleKey.UpArrow); // Checks if the up arrow key is pressed

}