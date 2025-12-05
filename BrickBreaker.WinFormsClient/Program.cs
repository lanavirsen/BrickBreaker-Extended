using System;
using System.Windows.Forms;
using BrickBreaker.WinFormsClient.WinUI;

namespace BrickBreaker.WinFormsClient;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LauncherForm());
    }
}
