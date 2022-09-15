using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TerrainParameters
{
    [SerializeField] private int chunkSize;
    [SerializeField] private int seed;
    [SerializeField] private int octaves;
    [SerializeField] private float scale;
    [SerializeField] private float frequency;
    [SerializeField] private Vector2 offset;
    [Range(0, 1)]
    [SerializeField] private float persistence;
    [SerializeField] private float lacunarity;
    //[SerializeField] private FastNoiseLite.NoiseType noiseType;
    //[SerializeField] private float gain;
    //[SerializeField] private float weightedStrength;
    [SerializeField] private float heightMultiplier;

    [Range(0, 6)]
    [SerializeField] private int levelOfDetail;
    [SerializeField] private NoiseNormalizationMode noiseNormalization;
    [SerializeField] private float noiseNormaliseFactor;
    [SerializeField] private bool randomSeed;

    public int ChunkSize { get { return chunkSize; } set { chunkSize = value; } }
    public int Seed
    {
        get
        {
            if (randomSeed)
            {
                System.Random random = new System.Random();
                seed = random.Next(-1000000, 1000000);
            }
            return seed;
        }
        set { seed = value; }
    }
    public int Octaves { get { return octaves; } set { octaves = value; } }
    //public float Gain { get { return gain; } set { gain = value; } }
    public float Scale { get { return scale; } set { scale = value; } }
    public float Frequency { get { return frequency; } set { frequency = value; } }
    public Vector2 Offset { get { return offset; } set { offset = value; } }
    public float Persistence { get { return persistence; } set { persistence = value; } }
    public float Lacunarity { get { return lacunarity; } set { lacunarity = value; } }
    //public FastNoiseLite.NoiseType NoiseType { get { return noiseType; } }
    //public float WeightedStrength { get { return weightedStrength; } set { weightedStrength = value; } }
    public float HeightMultiplier { get { return heightMultiplier; } set { heightMultiplier = value; } }
    public int LevelOfDetail { get { return levelOfDetail; } set { levelOfDetail = value; } }
    public NoiseNormalizationMode NoiseNormalization { get { return noiseNormalization; } set { noiseNormalization = value; } }
    public float NoiseNormaliseFactor { get { return noiseNormaliseFactor; } set { noiseNormaliseFactor = value; } }


}
