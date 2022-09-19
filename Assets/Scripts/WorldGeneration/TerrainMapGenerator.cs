using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using EventCallbacksSystem;

public class TerrainMapGenerator : MonoBehaviour
{
    private Queue<MapTreadInfo<TerrainMapHolder>> terrainMapHolderThreadInfo = new Queue<MapTreadInfo<TerrainMapHolder>>();
    private Queue<MapTreadInfo<MeshHolder>> meshHolderThreadInfo = new Queue<MapTreadInfo<MeshHolder>>();
    private float[,] fallOffMap;
    private float[,] heightMap;
    private ObjectPlacement objectPlacement;

    [SerializeField] private NoiseInfoHolder noiseInfo;
    [SerializeField] private TerrainInfoHolder terrainInfo;
    [SerializeField] private TextureInfoHolder textureInfo;
    [SerializeField] private RenderMode renderMode;
    [Range(0, 6)]
    [SerializeField] private int levelOfDetalInEditor;
    [SerializeField] private Material terrainMaterial;

    [SerializeField] private TerrainMapVisualize mapVisualizer;
    [SerializeField] private Biome[] biomes;
    [SerializeField] private List<GameObject> prefabs;

    public bool autoUpdate;
    public NoiseInfoHolder NoiseInfo { get { return noiseInfo; } }
    public TerrainInfoHolder TerrainInfo { get { return terrainInfo; } }
    public TextureInfoHolder TextureInfo { get { return textureInfo; } }
    public ObjectPlacement ObjectPlacement { get { return objectPlacement; } }

    private void Awake()
    {
        EventSystem.Instance.RegisterListener<UpdatedTerrainInfoEvent>(HandleUpdatedTerrainInfoEvent);
        EventSystem.Instance.RegisterListener<UpdatedNoiseInfoEvent>(HandleUpdatedNoiseInfoEvent);
        //EventSystem.Instance.RegisterListener<UpdatedTextureInfoEvent>(HandleUpdatedTextureInfoEvent);
        objectPlacement = FindObjectOfType<ObjectPlacement>();
    }

    private void HandleUpdatedTerrainInfoEvent(UpdatedTerrainInfoEvent ev)
    {
        if (Application.isPlaying == false)
        {
            RenderMapInEditor();
        }
    }

    private void HandleUpdatedNoiseInfoEvent(UpdatedNoiseInfoEvent ev)
    {
        if (Application.isPlaying == false)
        {
            RenderMapInEditor();
        }
    }

    //private void HandleUpdatedTextureInfoEvent(UpdatedTextureInfoEvent ev)
    //{
    //    textureInfo.ApplyToMaterial(terrainMaterial);
    //}

    public int MapChunkSize
    {
        get
        {
            if (terrainInfo.UseFlatShading == true)
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
            mapVisualizer.RenderMesh(MeshCreator.GenerateTerrainMesh(
                terrainMap.heightMap,
                terrainInfo.HeightMultiplier,
                terrainInfo.HeightCurve,
                levelOfDetalInEditor,
                terrainInfo.UseFlatShading),
                TextureGenerator.CreateColorMapTexture(
                    terrainMap.colorMap,
                    MapChunkSize,
                    MapChunkSize));
            PlaceObjects();
        }
        else if (renderMode == RenderMode.RenderFalloff)
        {
            mapVisualizer.RenderTexture(TextureGenerator.CreateHeightMapTexture(FalloffGenerator.GenerateFallOffMap(MapChunkSize)));
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
        MeshHolder meshHolder = MeshCreator.GenerateTerrainMesh(terrainMapHolder.heightMap, terrainInfo.HeightMultiplier, terrainInfo.HeightCurve, lod, terrainInfo.UseFlatShading);
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

    private void PlaceObjects()
    {
        //int chunkSize = MapChunkSize - 1;
        List<Vector2> points = ObjectPlacement.GeneratePoints(new Vector2(MapChunkSize, MapChunkSize), 10f, 30);
        Vector3 startPosSpawn = new Vector3(objectPlacement.transform.position.x - (float)(MapChunkSize / 2), 60f, objectPlacement.transform.position.z + (float)(MapChunkSize / 2));
        Vector3 posToSpawn = startPosSpawn;

        for (int i = 0; i < points.Count; i++)
        {
            posToSpawn.x += points[i].x;
            posToSpawn.z += points[i].y - MapChunkSize;

            Ray ray = new Ray(posToSpawn, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //GameObject tree = Instantiate(objectPlacement.TreePrefab, objectPlacement.transform);
                GameObject objectToPlace = Instantiate(prefabs[ObjectPlacement.RandomBetweenRangeInt(0, prefabs.Count)], objectPlacement.transform);
                objectToPlace.transform.position = hit.point - Vector3.up;
                objectToPlace.transform.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, 0), Quaternion.FromToRotation(Vector3.up, hit.normal), 0.5f);
                objectToPlace.transform.Rotate(Vector3.up, ObjectPlacement.RandomBetweenRange(0, 360));
                objectToPlace.transform.localScale *= ObjectPlacement.RandomBetweenRange(1f, 3f);
            }

            posToSpawn = startPosSpawn;
        }
    }


    private TerrainMapHolder GenerateTerrainMap(Vector2 chunkCentre)
    {
        float[,] terrainNoiseMap = NoiseGenerator.GenerateNoise(MapChunkSize + 2, chunkCentre, noiseInfo);
        heightMap = terrainNoiseMap;
        if (terrainInfo.UseFallOff)
        {
            fallOffMap = FalloffGenerator.GenerateFallOffMap(MapChunkSize + 2);
        }


        Color[] colorMap = new Color[(MapChunkSize + 2) * (MapChunkSize + 2)];
        for (int y = 0; y < MapChunkSize + 2; y++)
        {
            for (int x = 0; x < MapChunkSize + 2; x++)
            {
                if (terrainInfo.UseFallOff)
                {
                    terrainNoiseMap[x, y] = Mathf.Clamp01(terrainNoiseMap[x, y] - fallOffMap[x, y]);
                }

                float currentHeight = terrainNoiseMap[x, y];
                for (int i = 0; i < biomes.Length; i++)
                {
                    if (currentHeight >= biomes[i].BiomeHeight)
                    {
                        colorMap[y * MapChunkSize + 2 + x] = biomes[i].BiomeColor;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        List<Vector2> points = ObjectPlacement.GeneratePoints(new Vector2(MapChunkSize, MapChunkSize), 10f, 30);

        return new TerrainMapHolder(terrainNoiseMap, colorMap, points);
    }

    private void OnValidate()
    {

        if (EventSystem.Instance.HasRegisteredListener<UpdatedTerrainInfoEvent>(HandleUpdatedTerrainInfoEvent) == false)
        {
            EventSystem.Instance.RegisterListener<UpdatedTerrainInfoEvent>(HandleUpdatedTerrainInfoEvent);
        }

        if (EventSystem.Instance.HasRegisteredListener<UpdatedNoiseInfoEvent>(HandleUpdatedNoiseInfoEvent) == false)
        {
            EventSystem.Instance.RegisterListener<UpdatedNoiseInfoEvent>(HandleUpdatedNoiseInfoEvent);
        }

        if (objectPlacement == null)
        {
            objectPlacement = FindObjectOfType<ObjectPlacement>();
        }
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
    public readonly List<Vector2> placementPoints;

    public TerrainMapHolder(float[,] heightMaps, Color[] colorMap, List<Vector2> placementPoints)
    {
        this.heightMap = heightMaps;
        this.colorMap = colorMap;
        this.placementPoints = placementPoints;
    }
}

