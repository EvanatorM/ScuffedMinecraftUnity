using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "ScriptableObjects/Underground Noise Settings")]
public class UndergroundNoiseSettings : NoiseSettings
{
    public float chance;
    public int maxHeight;

    public int block;
}