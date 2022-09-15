using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class NoiseGenerator
{
    private const int MinRandomConstant = -100000;
    private const int MaxRandomConstant = 100000;
    private const float smallNumber = 0.0001f;

    public static float[,] GenerateNoise(TerrainParameters terrainParameters, Vector2 center)
    {
        Vector2 chunkCenterOffset = terrainParameters.Offset + center;
        float[,] noise = new float[terrainParameters.ChunkSize, terrainParameters.ChunkSize];

        System.Random pseudoRandom = new System.Random(terrainParameters.Seed);
        Vector2[] offsets = new Vector2[terrainParameters.Octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < terrainParameters.Octaves; i++)
        {
            float offsetX = pseudoRandom.Next(MinRandomConstant, MaxRandomConstant) + chunkCenterOffset.x;
            float offsetY = pseudoRandom.Next(MinRandomConstant, MaxRandomConstant) - chunkCenterOffset.y;
            offsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= terrainParameters.Persistence;
        }

        if (terrainParameters.Scale <= 0f)
        {
            terrainParameters.Scale = smallNumber;
        }

        float maxLocalNoiseValue = float.MinValue;
        float minLocalNoiseValue = float.MaxValue;

        float halfSize = terrainParameters.ChunkSize / 2;

        for (int y = 0; y < terrainParameters.ChunkSize; y++)
        {
            for (int x = 0; x < terrainParameters.ChunkSize; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseValue = 0;

                for (int i = 0; i < terrainParameters.Octaves; i++)
                {
                    float sampleX = (x - halfSize + offsets[i].x) / terrainParameters.Scale * frequency;
                    float sampleY = (y - halfSize + offsets[i].y) / terrainParameters.Scale * frequency;

                    float rawNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseValue += rawNoise * amplitude;


                    amplitude *= terrainParameters.Persistence;
                    frequency *= terrainParameters.Lacunarity;
                }

                if (noiseValue > maxLocalNoiseValue)
                {
                    maxLocalNoiseValue = noiseValue;
                }
                else if (noiseValue < minLocalNoiseValue)
                {
                    minLocalNoiseValue = noiseValue;
                }

                noise[x, y] = noiseValue;
            }
        }

        for (int y = 0; y < terrainParameters.ChunkSize; y++)
        {
            for (int x = 0; x < terrainParameters.ChunkSize; x++)
            {
                if (terrainParameters.NoiseNormalization == NoiseNormalizationMode.Local)
                {
                    noise[x, y] = Mathf.InverseLerp(minLocalNoiseValue, maxLocalNoiseValue, noise[x, y]);
                }
                else
                {
                    float normalizedHeight = (noise[x, y] + 1) / (maxPossibleHeight / terrainParameters.NoiseNormaliseFactor);
                    noise[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noise;
    }
}

public enum NoiseNormalizationMode
{
    Local,
    Global
}
