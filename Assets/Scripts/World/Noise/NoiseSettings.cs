using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Noise Settings")]
public class NoiseSettings : ScriptableObject
{
    public float scale = 1f;
    public float yMin = 1f;
    public float yMax = 1f;

    public int octaves = 4;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2f;

    public float amplitude = 1;
    public float frequency = 1;

    public Vector3 offset;

    public int topBlock;
    public int layer2Block;
}
