using UnityEngine;

namespace CameraOverhaul;

public static class CameraOverhaulApi
{
    private static float _externalSuppression;

    public static float ExternalSuppression
    {
        get => _externalSuppression;
        set => _externalSuppression = Mathf.Clamp01(value);
    }

    internal static float EffectScale => 1f - _externalSuppression;
}
