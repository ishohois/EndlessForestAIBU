using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RepeatingTerrain : MonoBehaviour
{
    private const float ViewerMovementChunkUpdateLimit = 25f;
    private const float sqrViewerMovementChunkUpdateLimit = ViewerMovementChunkUpdateLimit * ViewerMovementChunkUpdateLimit;
    private const int MaxNumberOfChunks = 30;

    private static float maxViewDistance;
    private static TerrainMapGenerator terrainMapGenerator;
    private static List<TerrainChunk> lastActiveChunks = new List<TerrainChunk>();
    private static int generatedChunks = 1;

    private int chunkSize;
    private int chunkVisibleInViewDistance;
    private Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    private Vector2 viewerPositionOld;

    [SerializeField] private Material mapMaterial;
    [SerializeField] private List<GameObject> vegetationPrefabs;

    public static Vector2 viewerPosition;


    public Transform viewer;

    private void Start()
    {
        terrainMapGenerator = FindObjectOfType<TerrainMapGenerator>();
        maxViewDistance = 250f;
        chunkSize = terrainMapGenerator.MapChunkSize - 1;
        chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition.x = viewer.position.x / terrainMapGenerator.TerrainInfo.UniformScale;
        viewerPosition.y = viewer.position.z / terrainMapGenerator.TerrainInfo.UniformScale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMovementChunkUpdateLimit)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

    }

    //private void GenerateChunks()
    //{
    //    int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
    //    int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
    //    Vector2 chunkInViewPositiom = Vector2.zero;
    //    for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++)
    //    {
    //        for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++)
    //        {
    //            chunkInViewPositiom.x = currentChunkCoordX + xOffset;
    //            chunkInViewPositiom.y = currentChunkCoordY + yOffset;
    //            terrainChunks.Add(chunkInViewPositiom, new TerrainChunk(chunkInViewPositiom, chunkSize,terrainMapGenerator.TerrainInfo.UniformScale, transform, mapMaterial, vegetationPrefabs));
               
    //        }
    //    }
    //}


    private void UpdateVisibleChunks()
    {
        for (int i = 0; i < lastActiveChunks.Count; i++)
        {
            lastActiveChunks[i].SetChunkVisibility(false);
        }

        //lastActiveChunks.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        Vector2 chunkInViewPositiom = Vector2.zero;
        for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++)
            {
                chunkInViewPositiom.x = currentChunkCoordX + xOffset;
                chunkInViewPositiom.y = currentChunkCoordY + yOffset;
                if (terrainChunks.ContainsKey(chunkInViewPositiom))
                {
                    terrainChunks[chunkInViewPositiom].UpdateChunk();
                }
                else
                {
                    terrainChunks.Add(chunkInViewPositiom, new TerrainChunk(chunkInViewPositiom, chunkSize, terrainMapGenerator.TerrainInfo.UniformScale, transform, mapMaterial, vegetationPrefabs));
                }
            }
        }

        if (generatedChunks >= MaxNumberOfChunks)
        {
            foreach (var chunk in lastActiveChunks)
            {
                if (chunk.IsVisible() == false)
                {
                    generatedChunks -= (generatedChunks % MaxNumberOfChunks);
                    chunk.DestroyChunk();
                }
            }
        }

    }

    private class TerrainChunk
    {
        public GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        TerrainMapHolder terrainMap;
        bool terrainMapReceived;
        int size;
        float scale;
        bool isChunkVisible;
        List<GameObject> prefabs;
        bool hasPlacedObjects;
        public TerrainChunk(Vector2 viewerPostion, int size, float scale, Transform parent, Material material, List<GameObject> vegetationPrefabs)
        {
            //Debug.Log("Generated chunks" + generatedChunks);
            //Debug.Break();
            this.scale = scale;
            this.size = size;
            this.prefabs = vegetationPrefabs;

            position = viewerPostion * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 vector3 = new Vector3(position.x, 0f, position.y);

            meshObject = new GameObject("Terrain chunk" + generatedChunks);
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = vector3 * terrainMapGenerator.TerrainInfo.UniformScale;
            //meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * terrainMapGenerator.TerrainInfo.UniformScale;
            //SetObjectVisibility(false);

            terrainMapGenerator.RequestTerrainMapHolder(position, OnTerrainMapHolderReceived);

            generatedChunks++;
        }

        private void OnTerrainMapHolderReceived(TerrainMapHolder terrainMapHolder)
        {
            this.terrainMap = terrainMapHolder;
            terrainMapReceived = true;

            terrainMapGenerator.RequestMeshHolder(terrainMap, 0, OnMeshHolderReceived);

            UpdateChunk();

        }

        private void OnMeshHolderReceived(MeshHolder meshHolder)
        {
            meshFilter.mesh = meshHolder.GenerateMesh();
            PlaceObjects(size * scale, meshObject.transform, prefabs);
        }

        private void SetObjectVisibility(bool isVisible)
        {
            isChunkVisible = isVisible;
            if (meshObject != null)
                meshObject.SetActive(isVisible);
        }

        public void PlaceObjects(float chunkSize, Transform chunkTransform, List<GameObject> prefabs)
        {
            meshCollider = meshObject.AddComponent<MeshCollider>();
            List<Vector2> points = ObjectPlacement.GeneratePoints(new Vector2(chunkSize * 2.5f, chunkSize), 20f, 30);
            Vector3 startPosSpawn = new Vector3(chunkTransform.transform.position.x - ((chunkSize)/ 2), 60f, chunkTransform.transform.position.z + ((chunkSize)/ 2));
            Vector3 posToSpawn = startPosSpawn;

            for (int i = 0; i < points.Count; i++)
            {
                posToSpawn.x += points[i].x;
                posToSpawn.z += points[i].y - chunkSize;

                Ray ray = new Ray(posToSpawn, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject objectToPlace = Instantiate(prefabs[ObjectPlacement.RandomBetweenRangeInt(0, prefabs.Count)], chunkTransform.transform);
                    objectToPlace.transform.position = hit.point - Vector3.up;
                    objectToPlace.transform.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, 0), Quaternion.FromToRotation(Vector3.up, hit.normal), 0.5f);
                    objectToPlace.transform.Rotate(Vector3.up, ObjectPlacement.RandomBetweenRange(0, 360));
                    //objectToPlace.transform.localScale *= ObjectPlacement.RandomBetweenRange(1f, 3f);
                }

                posToSpawn = startPosSpawn;
            }
        }

        private void ActivateVegetation(bool isVisible)
        {
            foreach (Transform child in meshObject.transform)
            {
                child.gameObject.SetActive(isVisible);
            }
        }


        public void UpdateChunk()
        {
            //Debug.Log("UpdateChunk()");
            //Debug.Break();
            if (terrainMapReceived == true)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible == true)
                {
                    lastActiveChunks.Add(this);
                }

                ActivateVegetation(visible);
                SetObjectVisibility(visible);
            }
        }

        public void SetChunkVisibility(bool isVisible)
        {
            SetObjectVisibility(isVisible);
        }

        public bool IsVisible()
        {
            return isChunkVisible;
        }


        public void DestroyChunk()
        {
            Destroy(meshObject);
        }
    }
           
}

