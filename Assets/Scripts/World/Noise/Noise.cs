using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float GetHeight(int seed, NoiseSettings settings, float x, float z)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];
        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x;
            float offsetY = prng.Next(-100000, 100000) + settings.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float amplitude = settings.amplitude;
        float frequency = settings.frequency;
        float noiseHeight = 0;

        for (int i = 0; i < settings.octaves; i++)
        {
            float sampleX = x / settings.scale * frequency + octaveOffsets[i].x;
            float sampleY = z / settings.scale * frequency + octaveOffsets[i].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;

            amplitude *= settings.persistance;
            frequency *= settings.lacunarity;
        }

        return Mathf.LerpUnclamped(settings.yMin, settings.yMax, noiseHeight);
    }
    public static float GetHeight(int seed, NoiseSettings settings, float x, float z, float minHeight, float maxHeight)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];
        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x;
            float offsetY = prng.Next(-100000, 100000) + settings.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float amplitude = settings.amplitude;
        float frequency = settings.frequency;
        float noiseHeight = 0;

        for (int i = 0; i < settings.octaves; i++)
        {
            float sampleX = x / settings.scale * frequency + octaveOffsets[i].x;
            float sampleY = z / settings.scale * frequency + octaveOffsets[i].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;

            amplitude *= settings.persistance;
            frequency *= settings.lacunarity;
        }

        return Mathf.LerpUnclamped(minHeight, maxHeight, noiseHeight);
    }

    public static float GetNoise3D(int seed, NoiseSettings settings, float x, float y, float z)
    {
        System.Random prng = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[settings.octaves];
        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x;
            float offsetY = prng.Next(-100000, 100000) + settings.offset.y;
            float offsetZ = prng.Next(-100000, 100000) + settings.offset.z;
            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
        }

        float amplitude = settings.amplitude;
        float frequency = settings.frequency;
        float noiseHeight = 0;

        for (int i = 0; i < settings.octaves; i++)
        {
            float sampleX = x / settings.scale * frequency + octaveOffsets[i].x;
            float sampleY = y / settings.scale * frequency + octaveOffsets[i].y;
            float sampleZ = z / settings.scale * frequency + octaveOffsets[i].z;

            float perlinValue = PerlinNoise3D(sampleX, sampleY, sampleZ);
            noiseHeight += perlinValue * amplitude;

            amplitude *= settings.persistance;
            frequency *= settings.lacunarity;
        }

        return noiseHeight * settings.yMin;
    }

    public static float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }
}
