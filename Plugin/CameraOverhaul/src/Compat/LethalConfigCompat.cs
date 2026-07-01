using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;

namespace CameraOverhaul;

internal static class LethalConfigCompat
{
    private const string Guid = ModGUIDs.LethalConfig;
    private static bool? _present;

    public static bool Present => _present ??= Chainloader.PluginInfos.ContainsKey(Guid);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Register(IReadOnlyList<ConfigEntryBase> entries)
    {
        foreach (ConfigEntryBase entry in entries)
        {
            switch (entry)
            {
                case ConfigEntry<float> f:
                    LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(f, requiresRestart: false));
                    break;
                case ConfigEntry<int> i:
                    LethalConfigManager.AddConfigItem(new IntSliderConfigItem(i, requiresRestart: false));
                    break;
                case ConfigEntry<bool> b:
                    LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(b, requiresRestart: false));
                    break;
                default:
                    Plugin.Log.LogDebug($"Skipping unsupported LethalConfig entry type: {entry.SettingType.Name}");
                    break;
            }
        }
    }
}