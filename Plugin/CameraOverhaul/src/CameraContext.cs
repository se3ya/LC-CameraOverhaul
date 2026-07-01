using UnityEngine;

namespace CameraOverhaul;

internal struct CameraContext
{
    public bool isSprinting;
    public bool isCrouching;
    public bool inVehicle;
    public bool isClimbing;
    public bool isExhausted;
    public bool isInspectingItem;
    public bool criticallyInjured;
    public bool isUsingJetpack;
    public bool isBeingShocked;
    public float sprintMeter;
    public float drunkness;
    public float insanity;
    public float poison;
    public float sinkingValue;
    public float shipTakeoffPhase;
    public float shipLandingPhase;

    public Vector3 velocity;
    public Vector3 forwardRelVelocity;

    public double pitch;
    public double yaw;

    public bool resetSmoothing;
}