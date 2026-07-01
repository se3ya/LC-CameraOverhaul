using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CameraOverhaul;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ModGUIDs.LethalConfig, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModGUIDs.ImmersiveVisor, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;
    public static ManualLogSource Log { get; private set; } = null!;

    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Instance = this;

        Log = base.Logger;

        Log.LogInfo($"Initializing {MyPluginInfo.PLUGIN_NAME}");

        ConfigManager.Initialize(Config);

        _harmony.PatchAll();

        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loaded!");
    }
}