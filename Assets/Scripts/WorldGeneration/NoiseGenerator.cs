using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class NoiseGenerator
{
    private const int MinRandomConstant = -100000;
    private const int MaxRandomConstant = 100000;

    public static float[,] GenerateNoise(int chunkSize, Vector2 center, NoiseInfoHolder noiseInfo)
    {
        Vector2 chunkCenterOffset = noiseInfo.Offset + center;
        float[,] noise = new float[chunkSize, chunkSize];

        System.Random pseudoRandom = new System.Random(noiseInfo.Seed);
        Vector2[] offsets = new Vector2[noiseInfo.Octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < noiseInfo.Octaves; i++)
        {
            float offsetX = pseudoRandom.Next(MinRandomConstant, MaxRandomConstant) + chunkCenterOffset.x;
            float offsetY = pseudoRandom.Next(MinRandomConstant, MaxRandomConstant) - chunkCenterOffset.y;
            offsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= noiseInfo.Persistence;
        }

        float maxLocalNoiseValue = float.MinValue;
        float minLocalNoiseValue = float.MaxValue;

        float halfSize = chunkSize / 2;

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseValue = 0;

                for (int i = 0; i < noiseInfo.Octaves; i++)
                {
                    float sampleX = (x - halfSize + offsets[i].x) / noiseInfo.NoiseScale * frequency;
                    float sampleY = (y - halfSize + offsets[i].y) / noiseInfo.NoiseScale * frequency;

                    float rawNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseValue += rawNoise * amplitude;


                    amplitude *= noiseInfo.Persistence;
                    frequency *= noiseInfo.Lacunarity;
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

                if (noiseInfo.NoiseNormalization == NoiseNormalizationMode.Global)
                {
                    float normalizedHeight = (noise[x, y] + 1) / (maxPossibleHeight / noiseInfo.NoiseNormaliseFactor);
                    noise[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        if (noiseInfo.NoiseNormalization == NoiseNormalizationMode.Local)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    noise[x, y] = Mathf.InverseLerp(minLocalNoiseValue, maxLocalNoiseValue, noise[x, y]);
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
