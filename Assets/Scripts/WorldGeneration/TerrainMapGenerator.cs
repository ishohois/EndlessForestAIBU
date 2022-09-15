using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMapGenerator : MonoBehaviour
{
    const int MaxMapChunkSize = 241;

    [SerializeField] private TerrainMapVisualize mapVisualizer;
    [SerializeField] private TerrainParameters terrainParameters;
    [SerializeField] private Biome[] biomes;
    [SerializeField] private RenderMode renderMode;
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private int numberOfChunks;


    public bool autoUpdate;

    public void GenerateTerrainMap()
    {
        float[,] terrainNoiseMap = NoiseGenerator.GenerateNoise(terrainParameters);


        Color[] colorMap = new Color[terrainParameters.ChunkSize * terrainParameters.ChunkSize];
        for (int y = 0; y < terrainParameters.ChunkSize; y++)
        {
            for (int x = 0; x < terrainParameters.ChunkSize; x++)
            {
                float currentHeight = terrainNoiseMap[x, y];
                for (int i = 0; i < biomes.Length; i++)
                {
                    if (currentHeight <= biomes[i].BiomeHeight)
                    {
                        colorMap[y * terrainParameters.ChunkSize + x] = biomes[i].BiomeColor;
                        break;
                    }
                }
            }
        }





        if (renderMode == RenderMode.RenderNoiseMap)
        {
            mapVisualizer.RenderTexture(TextureGenerator.CreateHeightMapTexture(terrainNoiseMap));
        }
        else if (renderMode == RenderMode.RenderColorMap)
        {
            mapVisualizer.RenderTexture(TextureGenerator.CreateColorMapTexture(colorMap, terrainParameters.ChunkSize, terrainParameters.ChunkSize));
        }
        else if (renderMode == RenderMode.RenderMesh)
        {
            mapVisualizer.RenderMesh(MeshCreator.GenerateTerrainMesh(terrainNoiseMap, terrainParameters.HeightMultiplier, heightCurve, terrainParameters.LevelOfDetail),
                TextureGenerator.CreateColorMapTexture(
                    colorMap,
                    terrainParameters.ChunkSize,
                    terrainParameters.ChunkSize));
        }
    }

    private void OnValidate()
    {

        if (terrainParameters.Lacunarity < 1)
        {
            terrainParameters.Lacunarity = 1f;
        }

        if (terrainParameters.Octaves < 0)
        {
            terrainParameters.Octaves = 1;
        }
    }
}

