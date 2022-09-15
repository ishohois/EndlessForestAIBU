using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class TerrainMapGenerator : MonoBehaviour
{
    public const int MaxMapChunkSize = 241;

    [SerializeField] private TerrainMapVisualize mapVisualizer;
    [SerializeField] private TerrainParameters terrainParameters;
    [SerializeField] private Biome[] biomes;
    [SerializeField] private RenderMode renderMode;
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private int numberOfChunks;
    [SerializeField] private bool useFallOff;

    private Queue<MapTreadInfo<TerrainMapHolder>> terrainMapHolderThreadInfo = new Queue<MapTreadInfo<TerrainMapHolder>>();
    private Queue<MapTreadInfo<MeshHolder>> meshHolderThreadInfo = new Queue<MapTreadInfo<MeshHolder>>();
    private float[,] fallOffMap;

    public bool autoUpdate;
    public TerrainParameters TerrainParameters { get { return terrainParameters; } }

    private void Awake()
    {
        fallOffMap = FalloffGenerator.GenerateFallOffMap(terrainParameters.ChunkSize);
    }

    public void RenderMapInEditor()
    {
        TerrainMapHolder terrainMap = GenerateTerrainMap(Vector2.zero);

        if (renderMode == RenderMode.RenderNoiseMap)
        {
            mapVisualizer.RenderTexture(TextureGenerator.CreateHeightMapTexture(terrainMap.heightMap));
        }
        else if (renderMode == RenderMode.RenderColorMap)
        {
            mapVisualizer.RenderTexture(TextureGenerator.CreateColorMapTexture(terrainMap.colorMap, terrainParameters.ChunkSize, terrainParameters.ChunkSize));
        }
        else if (renderMode == RenderMode.RenderMesh)
        {
            mapVisualizer.RenderMesh(MeshCreator.GenerateTerrainMesh(terrainMap.heightMap, terrainParameters.HeightMultiplier, heightCurve, terrainParameters.LevelOfDetail),
                TextureGenerator.CreateColorMapTexture(
                    terrainMap.colorMap,
                    terrainParameters.ChunkSize,
                    terrainParameters.ChunkSize));
        }
        else if (renderMode == RenderMode.RenderFalloff)
        {
            mapVisualizer.RenderTexture(TextureGenerator.CreateHeightMapTexture(fallOffMap));
        }
    }

    public void RequestTerrainMapHolder(Vector2 center, Action<TerrainMapHolder> callback)
    {
        ThreadStart threadStart = delegate
        {
            TerrainMapHolderThread(center, callback);
        };
        new Thread(threadStart).Start();
    }

    private void TerrainMapHolderThread(Vector2 center, Action<TerrainMapHolder> callback)
    {
        TerrainMapHolder terrainMapHolder = GenerateTerrainMap(center);
        lock (terrainMapHolderThreadInfo)
        {
            terrainMapHolderThreadInfo.Enqueue(new MapTreadInfo<TerrainMapHolder>(callback, terrainMapHolder));
        }
    }

    public void RequestMeshHolder(TerrainMapHolder terrainMapHolder, int lod, Action<MeshHolder> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshHolderThread(terrainMapHolder, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    private void MeshHolderThread(TerrainMapHolder terrainMapHolder, int lod, Action<MeshHolder> callback)
    {
        MeshHolder meshHolder = MeshCreator.GenerateTerrainMesh(terrainMapHolder.heightMap, terrainParameters.HeightMultiplier, heightCurve, lod);
        lock (meshHolderThreadInfo)
        {
            meshHolderThreadInfo.Enqueue(new MapTreadInfo<MeshHolder>(callback, meshHolder));
        }
    }

    private void Update()
    {
        if (terrainMapHolderThreadInfo.Count > 0)
        {
            for (int i = 0; i < terrainMapHolderThreadInfo.Count; i++)
            {
                MapTreadInfo<TerrainMapHolder> threadInfo = terrainMapHolderThreadInfo.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshHolderThreadInfo.Count > 0)
        {
            for (int i = 0; i < meshHolderThreadInfo.Count; i++)
            {
                MapTreadInfo<MeshHolder> threadInfo = meshHolderThreadInfo.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    private TerrainMapHolder GenerateTerrainMap(Vector2 chunkCentre)
    {
        float[,] terrainNoiseMap = NoiseGenerator.GenerateNoise(terrainParameters, chunkCentre);


        Color[] colorMap = new Color[terrainParameters.ChunkSize * terrainParameters.ChunkSize];
        for (int y = 0; y < terrainParameters.ChunkSize; y++)
        {
            for (int x = 0; x < terrainParameters.ChunkSize; x++)
            {
                if (useFallOff)
                {
                    terrainNoiseMap[x, y] = Mathf.Clamp01(terrainNoiseMap[x, y] - fallOffMap[x, y]);
                }

                float currentHeight = terrainNoiseMap[x, y];
                for (int i = 0; i < biomes.Length; i++)
                {
                    if (currentHeight >= biomes[i].BiomeHeight)
                    {
                        colorMap[y * terrainParameters.ChunkSize + x] = biomes[i].BiomeColor;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new TerrainMapHolder(terrainNoiseMap, colorMap);
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

        fallOffMap = FalloffGenerator.GenerateFallOffMap(terrainParameters.ChunkSize);
    }

    private struct MapTreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapTreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

public struct TerrainMapHolder
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public TerrainMapHolder(float[,] heightMaps, Color[] colorMap)
    {
        this.heightMap = heightMaps;
        this.colorMap = colorMap;
    }
}

