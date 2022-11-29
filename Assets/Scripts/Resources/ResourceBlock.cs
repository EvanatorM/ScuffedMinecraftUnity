using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBlock
{
    public int id;
    public string blockName;
    public Vector2[] texPosMin;
    public Vector2[] texPosMax;
    public bool sixSided;

    public bool transparent;

    public ResourceBlock(Block block, Vector2[] texPosMin, Vector2[] texPosMax)
    {
        id = block.id;
        blockName = block.blockName;
        this.texPosMin = texPosMin;
        this.texPosMax = texPosMax;
        sixSided = block.sixSided;

        transparent = block.transparent;
    }
}
