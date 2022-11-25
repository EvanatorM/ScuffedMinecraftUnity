using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public float amplitude;
    public float frequency;
}

public static class Noise
{
    public static float GetHeight(int seed, NoiseSettings settings, float x, float z)
    {
        x += seed + 0.01f;
        z += seed + 0.01f;
        return Mathf.PerlinNoise(x * settings.frequency, z * settings.frequency) * settings.amplitude;
    }
}
