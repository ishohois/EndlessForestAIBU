using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    private const float FactorA = 3f;
    private const float FactorB = 2.2f;

    public static float[,] GenerateFallOffMap(int size)
    {
        float[,] fallOffMap = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float xValue = x / (float)size * 2 - 1;
                float yValue = y / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(xValue), Mathf.Abs(yValue));
                fallOffMap[x, y] = Evaluate(value);
            }
        }

        return fallOffMap;
    }

    private static float Evaluate(float value)
    {
        return Mathf.Pow(value, FactorA) / (Mathf.Pow(value, FactorA) + Mathf.Pow(FactorB - FactorB * value, FactorA));
    }
}
