using System;

namespace CameraOverhaul;

internal static class MathUtils
{
    public static double Clamp(double value, double min, double max)
        => value < min ? min : (value > max ? max : value);

    public static double Clamp01(double value)
        => value < 0d ? 0d : (value > 1d ? 1d : value);

    public static double Lerp(double a, double b, double time)
        => a + (b - a) * Clamp01(time);

    // https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp
    public static double Damp(double source, double destination, double smoothing, double dt)
        => Lerp(source, destination, DampStep(smoothing, dt));

    public static double DampStep(double smoothing, double dt)
        => 1d - Math.Pow(smoothing * smoothing, dt);

    public static double StepTowards(double current, double target, double step)
    {
        if (current < target) return Math.Min(current + step, target);
        if (current > target) return Math.Max(current - step, target);
        return current;
    }

    public static double UnwrapStep(double d)
    {
        d %= 360.0;
        if (d <= -180.0) d += 360.0;
        else if (d > 180.0) d -= 360.0;
        return d;
    }
}