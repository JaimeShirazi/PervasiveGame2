//NOTE: The following code was generated with ChatGPT.
//It wasn't worth the time investment writing this logic from scratch for a simple prototype

using UnityEngine;
using System;

public static class MathUtils
{
    public static float SampleBeta(float a, float b)
    {
        float x = SampleGamma(a, 1f);
        float y = SampleGamma(b, 1f);
        return x / (x + y);
    }

    // Marsaglia and Tsang method for Gamma sampling (for a > 1)
    private static float SampleGamma(float shape, float scale)
    {
        if (shape < 1f)
        {
            // Use boost method: Gamma(a) = Gamma(a+1) * U^(1/a)
            float u = UnityEngine.Random.value;
            return SampleGamma(shape + 1f, scale) * Mathf.Pow(u, 1f / shape);
        }

        float d = shape - 1f / 3f;
        float c = 1f / Mathf.Sqrt(9f * d);

        while (true)
        {
            float x = NextGaussian();
            float v = 1f + c * x;
            if (v <= 0f) continue;
            v = v * v * v;

            float u = UnityEngine.Random.value;
            float x2 = x * x;

            if (u < 1f - 0.0331f * x2 * x2) return scale * d * v;
            if (Mathf.Log(u) < 0.5f * x2 + d * (1f - v + Mathf.Log(v)))
                return scale * d * v;
        }
    }

    // Standard normal distribution using Box-Muller transform
    private static float NextGaussian()
    {
        float u1 = UnityEngine.Random.value;
        float u2 = UnityEngine.Random.value;
        float r = Mathf.Sqrt(-2f * Mathf.Log(u1));
        float theta = 2f * Mathf.PI * u2;
        return r * Mathf.Cos(theta);
    }
}
