using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldGen
{
    public static NoiseSettings surfaceNoiseSettings;
    public static NoiseSettings[] undergroundNoiseSettings;
    public static int[] undergroundSettingsBlock;

    public static int GetBlockAtPos(int x, int y, int z, int seed)
    {
        int block;
        // Get height at position
        int height = Mathf.RoundToInt(Noise.GetHeight(seed, surfaceNoiseSettings, x, z));

        if (y == height) // Equal to height (Grass Layer)
            block = (int)Blocks.BLOCKS_BY_NAME.GRASS_BLOCK;
        else if (y < height - 4) // Less than 4 blocks below height (Stone Layer)
            block = (int)Blocks.BLOCKS_BY_NAME.STONE;
        else if (y < height) // Less than height (Dirt Layer)
            block = (int)Blocks.BLOCKS_BY_NAME.DIRT_BLOCK;
        else
            block = -1;
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

        for (int i = 0; i < undergroundNoiseSettings.Length; i++)
        {
            if (Noise.GetNoise3D(seed, undergroundNoiseSettings[i], x, y, z) <= undergroundNoiseSettings[i].chance)
            {
                if (y <= undergroundNoiseSettings[i].maxHeight)
                {
                    return undergroundSettingsBlock[i];
                }
            }
        }

        return block;
    }
}
