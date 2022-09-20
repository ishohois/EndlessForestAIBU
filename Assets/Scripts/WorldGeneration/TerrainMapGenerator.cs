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
    [SerializeField] private GameObject testMeshObject;

    [SerializeField] private NoiseInfoHolder noiseInfo;
    [SerializeField] private TerrainInfoHolder terrainInfo;
    [SerializeField] private TextureInfoHolder textureInfo;
    [SerializeField] private RenderMode renderMode;
    [Range(0, 6)]
    [SerializeField] private int levelOfDetalInEditor;
    [SerializeField] private Material terrainMaterial;

    [SerializeField] private TerrainMapVisualize mapVisualizer;
    //[SerializeField] private List<GameObject> prefabs;
    [SerializeField] private List<SpawnObject> prefabs;

    public bool autoUpdate;
    public NoiseInfoHolder NoiseInfo { get { return noiseInfo; } }
    public TerrainInfoHolder TerrainInfo { get { return terrainInfo; } }
    public TextureInfoHolder TextureInfo { get { return textureInfo; } }

    private void Awake()
    {
        EventSystem.Instance.RegisterListener<UpdatedTerrainInfoEvent>(HandleUpdatedTerrainInfoEvent);
        EventSystem.Instance.RegisterListener<UpdatedNoiseInfoEvent>(HandleUpdatedNoiseInfoEvent);
        //EventSystem.Instance.RegisterListener<UpdatedTextureInfoEvent>(HandleUpdatedTextureInfoEvent);
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
        else if (renderMode == RenderMode.RenderMesh)
        {
            mapVisualizer.RenderMesh(MeshCreator.GenerateTerrainMesh(
                terrainMap.heightMap,
                terrainInfo.HeightMultiplier,
                terrainInfo.HeightCurve,
                levelOfDetalInEditor,
                terrainInfo.UseFlatShading));
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
        List<Vector2> points = ObjectPlacement.GeneratePoints(new Vector2(MapChunkSize * terrainInfo.UniformScale, MapChunkSize * terrainInfo.UniformScale), 20f, 30);
        //List<Point> points = ObjectPlacement.GeneratePoints(1337, 10, MapChunkSize * terrainInfo.UniformScale, 30);
        Debug.Log("Mapchunksize " + MapChunkSize);
        Vector3 startPosSpawn = new Vector3(testMeshObject.transform.position.x - (float)((MapChunkSize * terrainInfo.UniformScale) / 2), 60f, testMeshObject.transform.position.z + (float)((MapChunkSize * terrainInfo.UniformScale) / 2));
        Vector3 posToSpawn = startPosSpawn;

        for (int i = 0; i < points.Count; i++)
        {
            posToSpawn.x += points[i].x;
            posToSpawn.z += points[i].y - MapChunkSize * terrainInfo.UniformScale;

            Ray ray = new Ray(posToSpawn, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject objectToPlace = Instantiate(prefabs[ObjectPlacement.RandomBetweenRangeInt(0, prefabs.Count)].Prefab, testMeshObject.transform);
                objectToPlace.transform.position = hit.point - Vector3.up;
                objectToPlace.transform.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, 0), Quaternion.FromToRotation(Vector3.up, hit.normal), 0.5f);
                objectToPlace.transform.Rotate(Vector3.up, ObjectPlacement.RandomBetweenRange(0, 360));
                objectToPlace.transform.localScale *= ObjectPlacement.RandomBetweenRange(0.2f, 1f);
            }

            posToSpawn = startPosSpawn;
        }
        Debug.Log("Number of points " + points.Count);
    }


    private TerrainMapHolder GenerateTerrainMap(Vector2 chunkCentre)
    {
        float[,] terrainNoiseMap = NoiseGenerator.GenerateNoise(MapChunkSize + 2, chunkCentre, noiseInfo);
        heightMap = terrainNoiseMap;
        if (terrainInfo.UseFallOff)
        {
            fallOffMap = FalloffGenerator.GenerateFallOffMap(MapChunkSize + 2);
        }


        for (int y = 0; y < MapChunkSize + 2; y++)
        {
            for (int x = 0; x < MapChunkSize + 2; x++)
            {
                if (terrainInfo.UseFallOff)
                {
                    terrainNoiseMap[x, y] = Mathf.Clamp01(terrainNoiseMap[x, y] - fallOffMap[x, y]);
                }

                float currentHeight = terrainNoiseMap[x, y];
            }
        }

        return new TerrainMapHolder(terrainNoiseMap);
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
    public TerrainMapHolder(float[,] heightMaps)
    {
        this.heightMap = heightMaps;
    }
}

[Serializable]
public class SpawnObject
{
    [SerializeField] private ObjectType objectType;
    [SerializeField] private GameObject prefab;
    [SerializeField] private float percentAmount;

    public ObjectType ObjectType { get { return objectType; } }
    public GameObject Prefab { get { return prefab; } }
    public float PercentAmount { get { return percentAmount; } }
}

public enum ObjectType
{
    Tree,
    Bush,
    Mushroom,
    Log,
}

