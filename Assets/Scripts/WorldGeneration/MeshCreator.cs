using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCreator
{
    public static MeshHolder GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail, bool useFlatShading)
    {
        AnimationCurve localHeightCurve = new AnimationCurve(heightCurve.keys);

        int simplificationFactor = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * simplificationFactor;
        int meshUnSimplified = borderedSize - 2;

        float topLeftX = (meshUnSimplified - 1) / -2f;
        float topLeftZ = (meshUnSimplified - 1) / 2f;

        int vertexCountPerLine = (meshSize - 1) / simplificationFactor + 1;

        MeshHolder meshHolder = new MeshHolder(vertexCountPerLine, useFlatShading);
        int[,] vertexIndexMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;


        for (int y = 0; y < borderedSize; y += simplificationFactor)
        {
            for (int x = 0; x < borderedSize; x += simplificationFactor)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndexMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndexMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }


        for (int y = 0; y < borderedSize; y += simplificationFactor)
        {
            for (int x = 0; x < borderedSize; x += simplificationFactor)
            {
                int vertexIndex = vertexIndexMap[x, y];
                Vector2 percent = new Vector2((x - simplificationFactor) / (float)meshSize, (y - simplificationFactor) / (float)meshSize);
                float height = localHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshUnSimplified, height, topLeftZ - percent.y * meshUnSimplified);

                meshHolder.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndexMap[x, y];
                    int b = vertexIndexMap[x + simplificationFactor, y];
                    int c = vertexIndexMap[x, y + simplificationFactor];
                    int d = vertexIndexMap[x + simplificationFactor, y + simplificationFactor];
                    meshHolder.AddTriangle(a, d, c);
                    meshHolder.AddTriangle(d, a, b);
                }

                vertexIndex++;
            }
        }

        meshHolder.ProcessMesh();

        return meshHolder;
    }
}

