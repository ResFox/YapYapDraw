using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YapYapDraw.Engine.ModuleSetup;

public static class ModuleConfig
{
    private static readonly Dictionary<string, object> Cache = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static bool IsEnabled(string? enableKey)
    {
        if (string.IsNullOrEmpty(enableKey))
            return true;

        var cfg = Plugin.Config;
        if (cfg.ModuleEnabled.TryGetValue(enableKey, out bool stored))
            return stored;

        return !cfg.DisabledMechanics.Contains(enableKey);
    }

    public static void SetEnabled(string? enableKey, bool enabled)
    {
        if (string.IsNullOrEmpty(enableKey))
            return;

        var cfg = Plugin.Config;
        cfg.ModuleEnabled[enableKey] = enabled;
        if (enabled)
            cfg.DisabledMechanics.Remove(enableKey);
        else
            cfg.DisabledMechanics.Add(enableKey);
        cfg.Save();
    }

    public static T Get<T>() where T : class, new()
    {
        string key = typeof(T).FullName!;
        if (Cache.TryGetValue(key, out object cached) && cached is T hit)
            return hit;

        T value = new();
        if (Plugin.Config.ModuleConfigs.TryGetValue(key, out string? json) && !string.IsNullOrEmpty(json))
        {
            try
            {
                value = JsonSerializer.Deserialize<T>(json, JsonOpts) ?? new T();
            }
            catch
            {
                value = new T();
            }
        }

        Cache[key] = value;
        return value;
    }

    public static void Set<T>(T value) where T : class, new()
    {
        string key = typeof(T).FullName!;
        Cache[key] = value;
        Plugin.Config.ModuleConfigs[key] = JsonSerializer.Serialize(value, JsonOpts);
        Plugin.Config.Save();
    }

    public static void Save<T>() where T : class, new()
    {
        string key = typeof(T).FullName!;
        if (!Cache.TryGetValue(key, out object cached) || cached is not T value)
            value = Get<T>();

        Plugin.Config.ModuleConfigs[key] = JsonSerializer.Serialize(value, JsonOpts);
        Plugin.Config.Save();
    }

    /// One-time import: old per-module JSON had Active=true but that blob can fail to reload.
    public static void MigrateLegacyActive(string enableKey, bool legacyActive)
    {
        if (string.IsNullOrEmpty(enableKey))
            return;
        if (Plugin.Config.ModuleEnabled.ContainsKey(enableKey))
            return;
        if (legacyActive)
            SetEnabled(enableKey, true);
    }
}
