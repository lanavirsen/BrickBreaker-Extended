using BrickBreaker.Models; 
using System.Collections.Generic; 
using System.IO; 
using System.Text.Json;

namespace BrickBreaker.Storage;

public sealed class LeaderboardStore
{
    private readonly string _path;

    public LeaderboardStore(string path) => _path = path;

    public void Add(ScoreEntry entry)
    {
        var entries = ReadAll();
        entries.Add(entry);
        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });

        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_path, json);
    }

    public List<ScoreEntry> ReadAll()
    {
        if (!File.Exists(_path))
            return new List<ScoreEntry>();

        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json))
            return new List<ScoreEntry>();

        try
        {
            return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
        }
        catch (JsonException)
        {
            return new List<ScoreEntry>();
        }
    }
}
