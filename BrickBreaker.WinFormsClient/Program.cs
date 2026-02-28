using BrickBreaker.WinFormsClient.WinUI;

namespace BrickBreaker.WinFormsClient;

internal static class Program
{
    // WinForms requires STA (single-threaded apartment) for its message loop
    // and COM interop with native controls.
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Must come after ApplicationConfiguration.Initialize(), which internally
        // calls SetCompatibleTextRenderingDefault(false). Switching to GDI+ (true)
        // fixes PressStart2P's clipped ascenders across all WinForms controls.
        Application.SetCompatibleTextRenderingDefault(true);
        Application.Run(new LauncherForm());
    }
}
