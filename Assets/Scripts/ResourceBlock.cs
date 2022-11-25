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

    public ResourceBlock(int id, string blockName, Vector2[] texPosMin, Vector2[] texPosMax, bool sixSided)
    {
        this.id = id;
        this.blockName = blockName;
        this.texPosMin = texPosMin;
        this.texPosMax = texPosMax;
        this.sixSided = sixSided;
    }
}
