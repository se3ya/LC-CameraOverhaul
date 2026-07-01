using UnityEngine;

namespace CameraOverhaul;

internal static class VisorCompat
{
    public static void StickVisor(Transform? localVisor, Transform? targetPoint, float strength)
    {
        if (localVisor == null || targetPoint == null) return;
        localVisor.rotation = strength >= 0.999f
            ? targetPoint.rotation
            : Quaternion.Slerp(localVisor.rotation, targetPoint.rotation, strength);
    }
}