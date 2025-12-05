using BrickBreaker.ConsoleClient.Ui.Enums;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using Spectre.Console;

namespace BrickBreaker.ConsoleClient.Ui.SpecterConsole
{
    // implementation of the login menu using Spectre.Console
    // displays options related to user login and registration
    public class LoginMenu : ILoginMenu
    {
        private readonly MenuHelper _menuHelper = new MenuHelper();

        public LoginMenuChoice Show()
        {
            AnsiConsole.Clear();
            var choice = _menuHelper.ShowMenu<LoginMenuChoice>("Brick Breaker");
            return choice;
        }
    }
}

