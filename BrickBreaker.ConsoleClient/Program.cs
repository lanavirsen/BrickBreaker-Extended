using BrickBreaker.ConsoleClient.Shell;

// Required for box-drawing and Unicode ball characters rendered during gameplay.
Console.OutputEncoding = System.Text.Encoding.UTF8;

// An optional API base URL can be passed as the first command-line argument,
// which overrides the value from appsettings (useful for local development).
string? preferredBase = args.FirstOrDefault(arg => !string.IsNullOrWhiteSpace(arg));
var dependencies = ConsoleShellDependencies.CreateDefault(preferredBase);

// ConsoleShell is IDisposable and owns the ApiClient lifetime, so `using` ensures
// the HTTP client is disposed cleanly when the app exits.
using var shell = new ConsoleShell(dependencies);
await shell.RunAsync();
