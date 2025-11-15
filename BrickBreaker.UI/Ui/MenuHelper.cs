using Microsoft.VisualBasic;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui
{
    public class MenuHelper
    {
        public T ShowMenu<T>(string title) where T : Enum
        {
            var options = Enum.GetNames(typeof(T));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{title}[/]")
                    .PageSize(10)
                    .AddChoices(options)
            );

            return (T)Enum.Parse(typeof(T), choice);
        }

        public void Pause(string message = "Press any key to continue...")
        {
            AnsiConsole.MarkupLine(message);
            Console.ReadKey(true);
        }
    }
}
