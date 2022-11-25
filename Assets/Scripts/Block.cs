using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Resources/Block")]
public class Block : ScriptableObject
{
    public int id;
    public string blockName;
    public Texture2D[] sideSprite;
    public bool sixSided;
}
