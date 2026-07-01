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
    public static void Register(IReadOnlyList<ConfigEntry<float>> floats, IReadOnlyList<ConfigEntry<bool>> bools)
    {
        foreach (ConfigEntry<float> e in floats)
            LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(e, requiresRestart: false));
        foreach (ConfigEntry<bool> e in bools)
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(e, requiresRestart: false));
    }
}