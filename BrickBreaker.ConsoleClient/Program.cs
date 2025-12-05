using System.Linq;
using BrickBreaker.ConsoleClient.Shell;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string? preferredBase = args.FirstOrDefault(arg => !string.IsNullOrWhiteSpace(arg));
var dependencies = ConsoleShellDependencies.CreateDefault(preferredBase);

using var shell = new ConsoleShell(dependencies);
await shell.RunAsync();
