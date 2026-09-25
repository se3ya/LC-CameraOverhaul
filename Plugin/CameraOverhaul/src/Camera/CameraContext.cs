using UnityEngine;

namespace CameraOverhaul;

internal struct CameraContext
{
    public bool isSprinting;
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
    public bool inWater;
    public bool submerged;
    public float leviathanProximity01;
    public LeviathanCue leviathanCue;
    public bool inSnow;
    public bool shipWithDoorsOpen;
    public bool hasActiveLight;
    public bool grabbedByEnemy;
    public bool grabbedByBracken;
    public bool isFalling;
    public bool gameBobEnabled;
    public bool hasStaticCharge;
    public float staticCharge;
    public float carryWeight;

    public Vector3 velocity;
    public Vector3 forwardRelVelocity;

    public double pitch;
    public double yaw;
    public double dtScale;

    public bool resetSmoothing;
}
