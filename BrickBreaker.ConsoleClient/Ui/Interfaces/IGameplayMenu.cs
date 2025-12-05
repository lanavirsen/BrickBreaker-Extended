using BrickBreaker.ConsoleClient.Ui.Enums;

namespace BrickBreaker.ConsoleClient.Ui.Interfaces
{
    // interface for displaying the gameplay menu
    public interface IGameplayMenu
    {
        GameplayMenuChoice Show(string username);
    }
}
