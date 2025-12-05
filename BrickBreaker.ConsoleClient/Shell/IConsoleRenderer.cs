using System;
using System.Threading.Tasks;

namespace BrickBreaker.ConsoleClient.Shell;

public interface IConsoleRenderer
{
    void ClearScreen();
    void RenderHeader();
    Task<T> RunStatusAsync<T>(string description, Func<Task<T>> action);
}
