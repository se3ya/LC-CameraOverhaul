using System;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;

namespace CameraOverhaul;

internal static class ShockTracker
{
    public static bool BeingShocked;

    internal static MethodBase ResolveInterfaceMethod(string name)
    {
        InterfaceMapping map = typeof(PlayerControllerB).GetInterfaceMap(typeof(IShockableWithGun));
        for (int i = 0; i < map.InterfaceMethods.Length; i++)
            if (map.InterfaceMethods[i].Name == name)
                return map.TargetMethods[i];
        throw new MissingMethodException($"PlayerControllerB.IShockableWithGun.{name} not found");
    }
}

[HarmonyPatch]
internal static class ShockStartPatch
{
    private static MethodBase TargetMethod() => ShockTracker.ResolveInterfaceMethod("ShockWithGun");

    [HarmonyPostfix]
    private static void Postfix(PlayerControllerB __instance)
    {
        if (__instance == StartOfRound.Instance?.localPlayerController)
            ShockTracker.BeingShocked = true;
    }
}

[HarmonyPatch]
internal static class ShockStopPatch
{
    private static MethodBase TargetMethod() => ShockTracker.ResolveInterfaceMethod("StopShockingWithGun");

    [HarmonyPostfix]
    private static void Postfix(PlayerControllerB __instance)
    {
        if (__instance == StartOfRound.Instance?.localPlayerController)
            ShockTracker.BeingShocked = false;
    }
}