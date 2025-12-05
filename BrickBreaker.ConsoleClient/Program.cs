using BrickBreaker.ConsoleClient.Shell;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var shell = new ConsoleShell();
await shell.RunAsync();
