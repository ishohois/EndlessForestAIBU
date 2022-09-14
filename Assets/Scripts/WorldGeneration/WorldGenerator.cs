using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    private float[,] noise;
    private MeshParameters landscapeLayer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [SerializeField] private TerrainParameters terrainParameters;

    private void InitializeMeshes()
    {
        landscapeLayer = new MeshParameters();
        landscapeLayer.InitializeMeshParameters(terrainParameters);
    }

    private void Awake()
    {

    }

    private void OnValidate()
    {
        GenerateWorld();
    }

    [ContextMenu("GenerateWorld")]
    private void GenerateWorld()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        meshFilter.mesh.Clear();
        InitializeMeshes();

        noise = new float[terrainParameters.Size, terrainParameters.Size];

        for (int y = 0; y < terrainParameters.Size; y++)
        {
            for (int x = 0; x < terrainParameters.Size; x++)
            {
                float amplitude = 1f;
                float frequency = 1f;

                float noiseValue = 0;

                for (int i = 0; i < terrainParameters.Octaves; i++)
                {
                    float sampleX = x / terrainParameters.Scale * frequency + terrainParameters.Offsets[i].x;
                    float sampleY = y / terrainParameters.Scale * frequency + terrainParameters.Offsets[i].y;

                    float rawNoise = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseValue += rawNoise * amplitude;

                    amplitude *= terrainParameters.Persistence;
                    frequency *= terrainParameters.Lacunarity;
                }
                noise[x, y] = noiseValue;

            }
        }

        float[] castNoise = noise.Cast<float>().ToArray();
        float max = castNoise.Max();
        float min = castNoise.Min();

        var triangles = new List<int>();
        int indexCount = 0;
        for (int y = 0; y < terrainParameters.Size; y++)
        {
            for (int x = 0; x < terrainParameters.Size; x++)
            {
                noise[x, y] = Mathf.InverseLerp(min, max, noise[x, y]);


                int i = (y * (terrainParameters.Size - 1)) + x + y;
                landscapeLayer.Vertices[i] = new Vector3(x, noise[x, y] * terrainParameters.HeightMultiplier, y);
                landscapeLayer.UVs[i] = new Vector2(x / (float)terrainParameters.Size, y / (float)terrainParameters.Size);

                if (x < terrainParameters.Size - 1 && y < terrainParameters.Size - 1)
                {
                    //first triangle
                    triangles.Add(i);
                    triangles.Add(i + terrainParameters.Size + 1);
                    triangles.Add(i + terrainParameters.Size);

                    // second triangle
                    triangles.Add(i + terrainParameters.Size + 1);
                    triangles.Add(i);
                    triangles.Add(i + 1);
                }
                indexCount = i;
            }
        }

        Debug.Log("Index count " + indexCount);
        Debug.Log("Triangles count " + triangles.Count);
        triangles.Reverse();
        landscapeLayer.Triangles = triangles.ToArray();
        Mesh mesh = new Mesh { vertices = landscapeLayer.Vertices, uv = landscapeLayer.UVs, triangles = landscapeLayer.Triangles };
        mesh.name = "LandscapeMesh";
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}
