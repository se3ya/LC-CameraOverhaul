using UnityEngine;

namespace CameraOverhaul;

internal enum NoiseKind
{
    Simplex,         // default for gentle sway.
    OpenSimplex2,    // improved gradient noise, very low directional artifacts.
    Perlin,          // unity built in, cheap. good for subtle secondary motion.
    PerlinWorley,    // soft.
    Fractal,         // 3 octave simplex fBm.
    FrequencyMod,    // harsh, rattly
}

internal static class Noise
{
    public static double Sample(NoiseKind kind, double x, double seed)
    {
        switch (kind)
        {
            case NoiseKind.Perlin: return Perlin((float)x, (float)seed);
            case NoiseKind.PerlinWorley: return PerlinWorley((float)x, (float)seed);
            case NoiseKind.Fractal: return Fractal((float)x, (float)seed);
            case NoiseKind.OpenSimplex2: return OpenSimplex2((float)x, (float)seed);
            case NoiseKind.FrequencyMod: return FrequencyMod((float)x, (float)seed);
            default: return Simplex((float)x, (float)seed);
        }
    }

    private static double Perlin(float x, float seed)
    {
        float v = Mathf.PerlinNoise(x + 0.137f, seed + 0.613f);
        return (v * 2.0) - 1.0;
    }

    private static float Fractal(float x, float y)
    {
        float sum = 0f, amp = 0.5f, freq = 1f, norm = 0f;
        for (int o = 0; o < 3; o++)
        {
            sum += amp * Simplex(x * freq, y * freq);
            norm += amp;
            amp *= 0.5f;
            freq *= 2f;
        }
        return sum / norm;
    }

    private static double PerlinWorley(float x, float seed)
    {
        double p = Perlin(x, seed);
        double w = (Worley1D((x * 0.5f), seed) * 2.0) - 1.0;
        return p * (0.5 + (0.5 * w));
    }

    private static double Worley1D(float x, float seed)
    {
        int cell = Mathf.FloorToInt(x);
        float f = x - cell;
        double d0 = Mathf.Abs(f - Hash01(cell, seed));
        double d1 = Mathf.Abs((f - 1f) - Hash01(cell + 1, seed));
        double d2 = Mathf.Abs((f + 1f) - Hash01(cell - 1, seed));
        double min = Mathf.Min((float)d0, Mathf.Min((float)d1, (float)d2));
        return 1.0 - Mathf.Min((float)min * 2f, 1f);
    }

    private static float Hash01(int i, float seed)
    {
        // integer hash to [0,1]
        int n = i ^ (int)(seed * 8192f);
        n = (n << 13) ^ n;
        n = (n * (((n * n) * 15731) + 789221)) + 1376312589;
        return ((n & 0x7fffffff) / 2147483647f);
    }

    private static double FrequencyMod(float x, float seed)
    {
        const float k = 3.7f;
        double s = System.Math.Sin(x + seed);
        return System.Math.Sin((s * k) + (x * 0.5));
    }

    private static readonly int[,] Grad = { { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 }, { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
    private static readonly int[] Perm = BuildPerm();

    private static int[] BuildPerm()
    {
        int[] p =
        {
            151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,
            23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,
            174,20,125,136,171,168,68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,
            133,230,220,105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,
            18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,5,202,
            38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,
            2,44,154,163,70,221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,
            104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,107,49,192,
            214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,
            243,141,128,195,78,66,215,61,156,180
        };
        int[] perm = new int[512];
        for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
        return perm;
    }

    private static float Simplex(float xin, float yin)
    {
        const float f2 = 0.366025403f; // 0.5*(sqrt(3)-1)
        const float g2 = 0.211324865f; // (3-sqrt(3))/6

        float s = (xin + yin) * f2;
        int i = FastFloor(xin + s);
        int j = FastFloor(yin + s);
        float t = (i + j) * g2;
        float x0 = xin - (i - t);
        float y0 = yin - (j - t);

        int i1 = x0 > y0 ? 1 : 0;
        int j1 = x0 > y0 ? 0 : 1;
        float x1 = x0 - i1 + g2;
        float y1 = y0 - j1 + g2;
        float x2 = (x0 - 1f) + (2f * g2);
        float y2 = (y0 - 1f) + (2f * g2);

        int ii = i & 255;
        int jj = j & 255;
        int gi0 = Perm[ii + Perm[jj]] % 8;
        int gi1 = Perm[ii + i1 + Perm[jj + j1]] % 8;
        int gi2 = Perm[ii + 1 + Perm[jj + 1]] % 8;

        return 70f * (Corner(x0, y0, gi0) + Corner(x1, y1, gi1) + Corner(x2, y2, gi2));
    }

    private static float Corner(float x, float y, int gi)
    {
        float t = 0.5f - (x * x) - (y * y);
        if (t < 0f) return 0f;
        t *= t;
        return (t * t) * ((Grad[gi, 0] * x) + (Grad[gi, 1] * y));
    }

    private static double OpenSimplex2(float x, float seed)
    {
        const float rot = 0.5773502692f; // 1/sqrt(3)
        float xr = (x + seed) * rot;
        float yr = seed * rot;
        return Simplex((xr * 1.4f), ((yr * 1.4f) + 17.3f));
    }

    private static int FastFloor(float v)
    {
        int i = (int)v;
        return v < i ? i - 1 : i;
    }
}