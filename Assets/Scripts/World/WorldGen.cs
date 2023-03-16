using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldGen
{
    public static NoiseSettings[] surfaceNoiseSettings;
    public static NoiseSettings temperatureSettings, humiditySettings;
    public static UndergroundNoiseSettings[] undergroundNoiseSettings;

    public static int GetBlockAtPos(int x, int y, int z, int seed)
    {
        int block;

        // Get Biome
        float temperature = Noise.GetHeight(seed, temperatureSettings, x, z);
        float humidity = Noise.GetHeight(seed, humiditySettings, x, z);

        int bX = GetClosestNumber(temperature, new float[] { 0f, 0.5f, 1f });
        int bY = GetClosestNumber(humidity, new float[] { 0f, 0.5f, 1f });
        //Debug.Log($"Temperature: {bX} ({temperature}), Humidity: {bY} ({humidity})");

        float minHeight = 0;
        float maxHeight = 0;

        for (int checkX = x - 5; checkX <= x + 5; checkX += 5)
        {
            for (int checkZ = z - 5; checkZ <= z + 5; checkZ += 5)
            {

                float cTemperature = Noise.GetHeight(seed, temperatureSettings, checkX, checkZ);
                float cHumidity = Noise.GetHeight(seed, humiditySettings, checkX, checkZ);

                int cX = GetClosestNumber(cTemperature, new float[] { 0f, 0.5f, 1f });
                int cY = GetClosestNumber(cHumidity, new float[] { 0f, 0.5f, 1f });

                minHeight += surfaceNoiseSettings[cY * 3 + cX].yMin;
                maxHeight += surfaceNoiseSettings[cY * 3 + cX].yMax;
            }
        }

        minHeight /= 9f;
        maxHeight /= 9f;

        // Get height at position
        int height = Mathf.RoundToInt(Noise.GetHeight(seed, surfaceNoiseSettings[bY * 3 + bX], x, z, minHeight, maxHeight));

        if (y == height) // Equal to height (Grass Layer)
            block = surfaceNoiseSettings[bY * 3 + bX].topBlock;
        else if (y < height - 4) // Less than 4 blocks below height (Stone Layer)
            block = (int)Blocks.BLOCKS_BY_NAME.STONE;
        else if (y < height) // Less than height (Dirt Layer)
            block = surfaceNoiseSettings[bY * 3 + bX].layer2Block;
        else
            block = -1;
        #region Trees
        /*else // Greater than height (Air)
        {
            float tree = Noise.GetHeight(seed, treeSettings, blockX, blockZ);
            System.Random rand = new System.Random(seed + (int)(tree * 5) + blockX + blockZ);
            int treeHeight = rand.Next(minTreeHeight, maxTreeHeight + 1);

            if (tree <= treeChance && blockY - height <= treeHeight)
                blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.LOG;
            else if (tree <= treeChance && blockY - height <= treeHeight + 3)
                blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.LEAVES;
            else
            {
                bool leaves = false;
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dz = -2; dz <= 2; dz++)
                    {
                        int checkHeight = Mathf.RoundToInt(Noise.GetHeight(seed, noiseSettings, blockX + dx, blockZ + dz));

                        tree = Noise.GetHeight(seed, treeSettings, blockX + dx, blockZ + dz);
                        rand = new System.Random(seed + (int)(tree * 5) + blockX + dx + blockZ + dz);
                        treeHeight = rand.Next(minTreeHeight, maxTreeHeight + 1);

                        if (tree > treeChance)
                            continue;

                        int dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz));
                        int dist2 = Mathf.Abs(dx) + Mathf.Abs(dz);

                        if (blockY - checkHeight > treeHeight - 2 && blockY - checkHeight <= treeHeight)
                        {
                            leaves = true;
                            break;
                        }
                        else if (blockY - checkHeight == treeHeight + 1 && dist2 < 4)
                        {
                            leaves = true;
                            break;
                        }
                        else if (blockY - checkHeight > treeHeight && blockY - checkHeight <= treeHeight + 2 && dist <= 1)
                        {
                            leaves = true;
                            break;
                        }
                        else if (blockY - checkHeight == treeHeight + 3 && dist2 == 1)
                        {
                            leaves = true;
                            break;
                        }
                    }

                    if (leaves)
                        break;
                }

                blocks[x, y, z] = leaves ? (int)Blocks.BLOCKS_BY_NAME.LEAVES : -1;
            }
        }*/
        #endregion

        for (int i = 0; i < undergroundNoiseSettings.Length; i++)
        {
            if (Noise.GetNoise3D(seed, undergroundNoiseSettings[i], x, y, z) <= undergroundNoiseSettings[i].chance)
            {
                if (y <= undergroundNoiseSettings[i].maxHeight)
                {
                    return undergroundNoiseSettings[i].block;
                }
            }
        }

        return block;
    }

    static int GetClosestNumber(float num, float[] numsClose)
    {
        int closestNum = 0;
        for (int i = 1; i < numsClose.Length; i++)
        {
            if (Mathf.Abs(num - numsClose[i]) < Mathf.Abs(num - numsClose[closestNum]))
                closestNum = i;
        }

        return closestNum;
    }
}
