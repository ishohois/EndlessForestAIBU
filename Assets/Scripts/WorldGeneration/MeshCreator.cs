using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCreator
{
    public static MeshHolder GenerateTerrainMesh(float[,] heightMap, float heightMultipier, AnimationCurve heightCurve, int levelOfDetail)
    {
        AnimationCurve localHeightCurve = new AnimationCurve(heightCurve.keys);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int simplificationFactor = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int vertexCountPerLine = (width - 1) / simplificationFactor + 1;

        MeshHolder meshHolder = new MeshHolder(vertexCountPerLine, vertexCountPerLine);
        int i = 0;
        for (int y = 0; y < height; y += simplificationFactor)
        {
            for (int x = 0; x < width; x += simplificationFactor)
            {
                meshHolder.Vertices[i] = new Vector3(topLeftX + x, localHeightCurve.Evaluate(heightMap[x, y]) * heightMultipier, topLeftZ - y);
                meshHolder.UVs[i] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshHolder.AddTriangles(i, i + vertexCountPerLine + 1, i + vertexCountPerLine);
                    meshHolder.AddTriangles(i + vertexCountPerLine + 1, i, i + 1);
                }

                i++;
            }
        }
        return meshHolder;
    }
}

