using System.Text.Json;

namespace AlchemyRPG;

/// <summary>
/// Represents the configuration settings for a game, including player information, dungeon theme, and log directory
/// </summary>
public class GameConfig
{
    public string PlayerName { get; set; } = "Hero";
    public string DungeonTheme { get; set; } = "Library";
    public string LogDirectory { get; set; } = "Logs";

    public static GameConfig Load(string path = "config.json")
    {
        try
        {
            if (!File.Exists(path))
            {
                var defaultConfig = new GameConfig();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string newJson = JsonSerializer.Serialize(defaultConfig, options);
                File.WriteAllText(path, newJson);

                return defaultConfig;
            }

            string existingJson = File.ReadAllText(path);
            var loadedConfig = JsonSerializer.Deserialize<GameConfig>(existingJson);

            return loadedConfig ?? new GameConfig();
        }
        catch (Exception)
        {
            return new GameConfig();
        }
    }
}
