using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using EventCallbacksSystem;

public class TerrainMapGenerator : MonoBehaviour
{

    [SerializeField] private TerrainMapVisualize mapVisualizer;
    [SerializeField] private TerrainParameters terrainParameters;
    [SerializeField] private Biome[] biomes;
    [SerializeField] private RenderMode renderMode;
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private int numberOfChunks;
    [SerializeField] private bool useFallOff;
    [SerializeField] private bool useFlatShading;

    [SerializeField] private TerrainInfoHolder terrainInfo;

    private Queue<MapTreadInfo<TerrainMapHolder>> terrainMapHolderThreadInfo = new Queue<MapTreadInfo<TerrainMapHolder>>();
    private Queue<MapTreadInfo<MeshHolder>> meshHolderThreadInfo = new Queue<MapTreadInfo<MeshHolder>>();
    private float[,] fallOffMap;
    private static TerrainMapGenerator mapGeneratorInstance;

    public bool autoUpdate;
    public TerrainParameters TerrainParameters { get { return terrainParameters; } }

    private void Awake()
    {
        fallOffMap = FalloffGenerator.GenerateFallOffMap(MapChunkSize);
        EventSystem.Instance.RegisterListener<UpdatedTerrainInfoEvent>(HandleUpdatedTerrainInfoEvent);
    }

    private void HandleUpdatedTerrainInfoEvent(UpdatedTerrainInfoEvent ev)
    {
        Debug.Log("Received the terrain info and will do something with the info");
    }

    public static int MapChunkSize
    {
        get
        {
            if (mapGeneratorInstance == null)
            {
                mapGeneratorInstance = FindObjectOfType<TerrainMapGenerator>();
            }
            if (mapGeneratorInstance.useFlatShading == true)
            {
                return 95;
            }
            else
            {
                return 239;
            }
        }
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
            mapVisualizer.RenderTexture(TextureGenerator.CreateColorMapTexture(terrainMap.colorMap, MapChunkSize, MapChunkSize));
        }
        else if (renderMode == RenderMode.RenderMesh)
        {
            mapVisualizer.RenderMesh(MeshCreator.GenerateTerrainMesh(terrainMap.heightMap, terrainParameters.HeightMultiplier, heightCurve, terrainParameters.LevelOfDetail, useFlatShading),
                TextureGenerator.CreateColorMapTexture(
                    terrainMap.colorMap,
                    MapChunkSize,
                    MapChunkSize));
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
        MeshHolder meshHolder = MeshCreator.GenerateTerrainMesh(terrainMapHolder.heightMap, terrainParameters.HeightMultiplier, heightCurve, lod, useFlatShading);
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


        Color[] colorMap = new Color[MapChunkSize * MapChunkSize];
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
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
                        colorMap[y * MapChunkSize + x] = biomes[i].BiomeColor;
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

        if (EventSystem.Instance.HasRegisteredListener<UpdatedTerrainInfoEvent>(HandleUpdatedTerrainInfoEvent) == false)
        {
            EventSystem.Instance.RegisterListener<UpdatedTerrainInfoEvent>(HandleUpdatedTerrainInfoEvent);
        }

        if (terrainParameters.Lacunarity < 1)
        {
            terrainParameters.Lacunarity = 1f;
        }

        if (terrainParameters.Octaves < 0)
        {
            terrainParameters.Octaves = 1;
        }

        fallOffMap = FalloffGenerator.GenerateFallOffMap(MapChunkSize);
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

