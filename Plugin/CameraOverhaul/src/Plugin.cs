using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ModGUIDs.LethalConfig, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModGUIDs.ImmersiveVisor, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;
    public static ManualLogSource Log { get; private set; } = null!;

    private const string GlobalConfigFolder = "CameraOverhaul";

    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Instance = this;

        Log = base.Logger;

        Log.LogInfo($"Initializing {MyPluginInfo.PLUGIN_NAME}");

        ConfigManager.Initialize(GetConfigFile());

        _harmony.PatchAll();

        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loaded!");
    }

    private ConfigFile GetConfigFile()
    {
        BepInPlugin metadata = MetadataHelper.GetMetadata(this);

        var bootstrap = new ConfigFile(
            Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_GUID + ".bootstrap.cfg"), true, metadata);
        bool useGlobalConfig = bootstrap.Bind("Main", "Use Global Config", false,
            "Share settings across all profiles via one global file instead of this profile's BepInEx/config. Requires a restart.").Value;

        string localPath = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_GUID + ".cfg");
        if (!useGlobalConfig)
            return new ConfigFile(localPath, false, metadata);

        string globalDirectory = Path.Combine(Application.persistentDataPath, GlobalConfigFolder);
        string globalPath = Path.Combine(globalDirectory, "global.cfg");
        MigrateLocalConfig(localPath, globalDirectory, globalPath);
        return new ConfigFile(globalPath, false, metadata);
    }

    private static void MigrateLocalConfig(string localPath, string globalDirectory, string globalPath)
    {
        if (File.Exists(globalPath) || !File.Exists(localPath))
            return;

        Directory.CreateDirectory(globalDirectory);
        File.Copy(localPath, globalPath);
        Log.LogInfo("Migrated existing profile config into the global config file.");
    }
}
