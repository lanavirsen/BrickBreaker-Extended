using System;

namespace BrickBreaker.Tests;

internal sealed class TempJsonFile : IDisposable
{
    private readonly string _directory;
    public string Path { get; }

    public TempJsonFile(string? initialJson = null)
    {
        _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "BrickBreakerTests",
            Guid.NewGuid().ToString("N"));

        System.IO.Directory.CreateDirectory(_directory);
        Path = System.IO.Path.Combine(_directory, "data.json");

        if (initialJson is not null)
        {
            System.IO.File.WriteAllText(Path, initialJson);
        }
    }

    public void Dispose()
    {
        try
        {
            if (System.IO.Directory.Exists(_directory))
            {
                System.IO.Directory.Delete(_directory, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup; ignore IO errors so tests can proceed.
        }
    }
}

