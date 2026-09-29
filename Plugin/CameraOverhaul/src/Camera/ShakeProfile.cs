namespace CameraOverhaul;

internal readonly struct ShakeProfile
{
    public readonly float Amount;
    public readonly float Length;
    public readonly float PeakAt;
    public readonly float Floor;
    public readonly float Angle;
    public readonly float Spread;
    public readonly float Rate;
    public readonly float Vary;
    public readonly bool Flip;

    public ShakeProfile(float amount, float length, float peakAt, float floor,
                        float angle, float spread, float rate, float vary, bool flip)
    {
        Amount = amount;
        Length = length;
        PeakAt = peakAt;
        Floor = floor;
        Angle = angle;
        Spread = spread;
        Rate = rate;
        Vary = vary;
        Flip = flip;
    }
}
