using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ObjectPlacement
{
    private const int seed = 1337;
    private static System.Random random;

    public static List<Vector2> GeneratePoints(Vector2 sampleRegionSize, float minimumDistanceBetweenPoints, int rejectionTries)
    {
        random = new System.Random(seed);
        float gridCell = minimumDistanceBetweenPoints / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / gridCell), Mathf.CeilToInt(sampleRegionSize.y / gridCell)];
        List<Vector2> validPoints = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize / 2);
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = random.Next(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < rejectionTries; i++)
            {
                float angle = RandomValue() * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidatePoint = spawnCentre + dir * RandomBetweenRange(minimumDistanceBetweenPoints, (2 * minimumDistanceBetweenPoints));
                if (IsValid(candidatePoint, sampleRegionSize, gridCell, minimumDistanceBetweenPoints, validPoints, grid))
                {
                    validPoints.Add(candidatePoint);
                    spawnPoints.Add(candidatePoint);
                    grid[(int)(candidatePoint.x / gridCell), (int)(candidatePoint.y / gridCell)] = validPoints.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }

        }

        return validPoints;
    }

    private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Min and Max Inclusive
    /// </summary>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static float RandomBetweenRange(float minimum, float maximum)
    {
        double min = minimum;
        double max = maximum;
        double range = max - min;
        double sample = random.NextDouble();
        double scaled = (sample * range) + min;

        return (float)scaled;
    }

    public static int RandomBetweenRangeInt(int minimum, int maximum)
    {
        return random.Next(minimum, maximum);
    }

    /// <summary>
    /// Get random float value between 0-1
    /// </summary>
    /// <returns></returns>
    public static float RandomValue()
    {
        return (float)random.NextDouble();
    }
}

