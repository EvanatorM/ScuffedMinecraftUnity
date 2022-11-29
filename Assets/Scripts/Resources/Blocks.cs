using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Blocks
{
    public static ResourceBlock[] BLOCKS = new ResourceBlock[]
    {
        //new ResourceBlock(0, "Dirt Block", new Vector2(0f, 0f), new Vector2(0.25f, 0.25f)),
        //new ResourceBlock(1, "Grass Block", new Vector2(0.25f, 0f), new Vector2(0.5f, 0.25f))
    };

    public enum BLOCKS_BY_NAME
    {
        DIRT_BLOCK,
        GRASS_BLOCK,
        STONE,
        LOG,
        LEAVES
    }

    public static void LoadResources()
    {
        Block[] blocks = Resources.LoadAll<Block>("Blocks/");
        Texture2D[] textures = Resources.LoadAll<Texture2D>("Blocks/");
        Debug.Log("Loaded Blocks: " + blocks.Length);
        Debug.Log("Loaded Textures: " + textures.Length);

        int spriteMapWidth = Mathf.CeilToInt(textures.Length / 2f) * 16;
        int spriteMapHeight = Mathf.CeilToInt(textures.Length / 2f) * 16;
        Debug.Log($"Map Size: ({spriteMapWidth}, {spriteMapHeight})");

        Texture2D spriteMap = new Texture2D(spriteMapWidth, spriteMapHeight, TextureFormat.ARGB32, false);

        int texX = -16;
        int texY = 0;
        for (int i = 0; i < textures.Length; i++)
        {
            texX += 16;
            if (texX >= spriteMapWidth)
            {
                texX = 0;
                texY += 16;
            }

            for (int x = 0; x < textures[i].width; x++)
            {
                for (int y = 0; y < textures[i].width; y++)
                {
                    spriteMap.SetPixel(texX + x, texY + y, textures[i].GetPixel(x, y));
                }
            }
        }

        spriteMap.filterMode = FilterMode.Point;
        spriteMap.Apply();

        Material spriteMaterial = (Material)Resources.Load("Blocks/SpriteMaterial");
        Material transparentSpriteMaterial = (Material)Resources.Load("Blocks/TransparentSpriteMaterial");
        spriteMaterial.mainTexture = spriteMap;
        transparentSpriteMaterial.mainTexture = spriteMap;

        List<ResourceBlock> tempBlocks = new List<ResourceBlock>();
        for (int i = 0; i < blocks.Length; i++)
        {
            Vector2[] texPosMin = new Vector2[blocks[i].sideSprite.Length];
            Vector2[] texPosMax = new Vector2[blocks[i].sideSprite.Length];

            for (int s = 0; s < texPosMin.Length; s++)
            {
                int texIndex = 0;
                for (int t = 0; t < textures.Length; t++)
                {
                    if (textures[t] == blocks[i].sideSprite[s])
                    {
                        texIndex = t;
                        break;
                    }
                }

                int texPosX = texIndex % (spriteMapWidth / 16);
                int texPosY = Mathf.FloorToInt(texIndex / (spriteMapWidth / 16f));

                texPosMin[s] = new Vector2(texPosX * 16f / spriteMapWidth, texPosY * 16f / spriteMapHeight);
                texPosMax[s] = new Vector2((texPosX * 16f + 16f) / spriteMapWidth, (texPosY * 16f + 16f) / spriteMapHeight);
            }

            tempBlocks.Add(new ResourceBlock(blocks[i], texPosMin, texPosMax));
        }

        BLOCKS = new ResourceBlock[tempBlocks.Count];
        for (int i = 0; i < tempBlocks.Count; i++)
        {
            BLOCKS[tempBlocks[i].id] = tempBlocks[i];
        }

        for (int i = 0; i < BLOCKS.Length; i++)
        {
            Debug.Log($"Block ID: {i}[{BLOCKS[i].id}]: {BLOCKS[i].blockName}");
        }

        foreach (Block block in blocks)
            Resources.UnloadAsset(block);
        foreach (Texture2D tex in textures)
            Resources.UnloadAsset(tex);
    }
}
