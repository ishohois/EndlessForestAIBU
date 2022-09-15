using UnityEngine;
using System;

[Serializable]
public class Biome
{
   [SerializeField] private BiomeType biomeType;
   [SerializeField] private float biomeHeight;
   [SerializeField] private Color biomeColor;

    public BiomeType BiomeType { get { return biomeType; } }
    public float BiomeHeight { get { return biomeHeight; } }
    public Color BiomeColor { get { return biomeColor; } }
}

public enum BiomeType
{
    Water,
    TreeHill,
    Grass,
    Forest,
    Mountain,
}
