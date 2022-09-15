using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class NoiseGenerator
{
    private const int MinRandomConstant = -100000;
    private const int MaxRandomConstant = 100000;
    private const float smallNumber = 0.0001f;

    public static float[,] GenerateNoise(TerrainParameters terrainParameters)
    {
        float[,] noise = new float[terrainParameters.ChunkSize, terrainParameters.ChunkSize];
        //FastNoiseLite noiseGenerator = new FastNoiseLite();

        //noiseGenerator.SetSeed(terrainParameters.Seed);
        //noiseGenerator.SetFrequency(terrainParameters.Frequency);
        //noiseGenerator.SetNoiseType(terrainParameters.NoiseType);
        //noiseGenerator.SetFractalOctaves(terrainParameters.Octaves);
        //noiseGenerator.SetFractalLacunarity(terrainParameters.Lacunarity);
        //noiseGenerator.SetFractalGain(terrainParameters.Gain);
        //noiseGenerator.SetFractalWeightedStrength(terrainParameters.WeightedStrength);

        System.Random pseudoRandom = new System.Random(terrainParameters.Seed);
        Vector2[] offsets = new Vector2[terrainParameters.Octaves];

        for (int i = 0; i < terrainParameters.Octaves; i++)
        {
            float offsetX = pseudoRandom.Next(MinRandomConstant, MaxRandomConstant) + terrainParameters.Offset.x;
            float offsetY = pseudoRandom.Next(MinRandomConstant, MaxRandomConstant) + terrainParameters.Offset.y;
            offsets[i] = new Vector2(offsetX, offsetY);
        }

        if (terrainParameters.Scale <= 0f)
        {
            terrainParameters.Scale = smallNumber;
        }

        float maxNoiseValue = float.MinValue;
        float minNoiseValue = float.MaxValue;

        for (int y = 0; y < terrainParameters.ChunkSize; y++)
        {
            for (int x = 0; x < terrainParameters.ChunkSize; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseValue = 0;

                for (int i = 0; i < terrainParameters.Octaves; i++)
                {
                    float sampleX = (x - terrainParameters.ChunkSize / 2) / terrainParameters.Scale * frequency + offsets[i].x;
                    float sampleY = (y - terrainParameters.ChunkSize / 2) / terrainParameters.Scale * frequency + offsets[i].y;

                    float rawNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseValue += rawNoise * amplitude;


                    amplitude *= terrainParameters.Persistence;
                    frequency *= terrainParameters.Lacunarity;
                }

                //noise[x, y] = noiseGenerator.GetNoise(sampleX, sampleY);

                if (noiseValue > maxNoiseValue)
                {
                    maxNoiseValue = noiseValue;
                }
                else if (noiseValue < minNoiseValue)
                {
                    minNoiseValue = noiseValue;
                }

                noise[x, y] = noiseValue;
            }
        }

        for (int y = 0; y < terrainParameters.ChunkSize; y++)
        {
            for (int x = 0; x < terrainParameters.ChunkSize; x++)
            {
                noise[x, y] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noise[x, y]);
            }
        }

        return noise;
    }
}
