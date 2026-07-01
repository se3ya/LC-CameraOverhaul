using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CameraOverhaul;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ModGUIDs.LethalConfig, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModGUIDs.ImmersiveVisor, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        Log.LogInfo($"Initializing {MyPluginInfo.PLUGIN_NAME}");

        ConfigManager.Initialize(Config);
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loaded!");
    }
}