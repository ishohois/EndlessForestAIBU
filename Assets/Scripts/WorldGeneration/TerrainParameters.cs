using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TerrainParameters
{
    [SerializeField] private int seed;
    [SerializeField] private int size;
    [SerializeField] private float scale;
    [SerializeField] private Vector2[] offsets;
    [SerializeField] private int heightMultiplier;
    [SerializeField] private int octaves;
    [SerializeField] private float persistence;
    [SerializeField] private float lacunarity;

    public int Seed { get { return seed; } set { seed = value; } }
    public int Size { get { return size; } set { size = value; } }
    public float Scale { get { return scale; } set { scale = value; } }
    public Vector2[] Offsets { get { return offsets; } set { offsets = value; } }
    public int HeightMultiplier { get { return heightMultiplier; } set { heightMultiplier = value; } }
    public int Octaves { get { return octaves; } set { octaves = value; } }
    public float Persistence { get { return persistence; } set { persistence = value; } }
    public float Lacunarity { get { return lacunarity; } set { lacunarity = value; } }

}
