using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{
    [SerializeField] int mapWidth, mapHeight;
    [SerializeField] NoiseSettings settings;

    [SerializeField] int seed;
    [SerializeField] Vector2 offset;

    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = TestNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, settings.scale, settings.octaves, settings.persistance, settings.lacunarity, offset);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }

    void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;
        if (mapHeight < 1)
            mapHeight = 1;
        if (settings.lacunarity < 1)
            settings.lacunarity = 1;
        if (settings.octaves < 0)
            settings.octaves = 0;
    }
}
