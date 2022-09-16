using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RepeatingTerrain : MonoBehaviour
{
    private const float Scale = 2f;
    private const float ViewerMovementChunkUpdateLimit = 25f;
    private const float sqrViewerMovementChunkUpdateLimit = ViewerMovementChunkUpdateLimit * ViewerMovementChunkUpdateLimit;

    private static float maxViewDistance;
    private static TerrainMapGenerator terrainMapGenerator;
    private static List<TerrainChunk> lastActiveChunks = new List<TerrainChunk>();

    private int chunkSize;
    private int chunkVisibleInViewDistance;
    private Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    private Vector2 viewerPositionOld;

    [SerializeField] private Material mapMaterial;
    [SerializeField] private LODInfo[] detailLevels;
    [SerializeField] private LayerMask groundLayer;

    private static LayerMask groundLayerMask;

    public static Vector2 viewerPosition;

    public Transform viewer;

    private void Start()
    {
        terrainMapGenerator = FindObjectOfType<TerrainMapGenerator>();
        groundLayerMask = groundLayer;
        maxViewDistance = detailLevels[detailLevels.Length - 1].VisibleDistanceLimit;
        chunkSize = TerrainMapGenerator.MapChunkSize - 1;
        chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition.x = viewer.position.x / Scale;
        viewerPosition.y = viewer.position.z / Scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMovementChunkUpdateLimit)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

    }

    private void UpdateVisibleChunks()
    {
        for (int i = 0; i < lastActiveChunks.Count; i++)
        {
            lastActiveChunks[i].SetChunkVisibility(false);
        }

        lastActiveChunks.Clear();

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
                    terrainChunks.Add(chunkInViewPositiom, new TerrainChunk(chunkInViewPositiom, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    private class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;

        TerrainMapHolder terrainMap;
        bool terrainMapReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 viewerPostion, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = viewerPostion * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 vector3 = new Vector3(position.x, 0f, position.y);

            meshObject = new GameObject("Terrain chunk");
            meshObject.layer = LayerMask.NameToLayer("Obstacle");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = vector3 * Scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * Scale;
            SetObjectVisibility(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].Lod, UpdateChunk);
                if (detailLevels[i].UseForCollider == true)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }

            terrainMapGenerator.RequestTerrainMapHolder(position, OnTerrainMapHolderReceived);
        }

        private void OnTerrainMapHolderReceived(TerrainMapHolder terrainMapHolder)
        {
            this.terrainMap = terrainMapHolder;
            terrainMapReceived = true;

            Texture2D texture = TextureGenerator.CreateColorMapTexture(
                terrainMap.colorMap,
                TerrainMapGenerator.MapChunkSize,
                TerrainMapGenerator.MapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateChunk();
        }

        private void SetObjectVisibility(bool isVisible)
        {
            meshObject.SetActive(isVisible);
        }

        public void UpdateChunk()
        {
            if (terrainMapReceived == true)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible == true)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].VisibleDistanceLimit)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh == true)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (lodMesh.hasRequestedMesh == false)
                        {
                            lodMesh.RequestMesh(terrainMap);
                        }
                    }

                    if (lodIndex == 0)
                    {
                        if (collisionLODMesh.hasMesh == true)
                        {
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        }
                        else if (collisionLODMesh.hasRequestedMesh == false)
                        {
                            collisionLODMesh.RequestMesh(terrainMap);
                        }
                    }

                    lastActiveChunks.Add(this);
                }

                SetObjectVisibility(visible);
            }
        }

        public void SetChunkVisibility(bool isVisible)
        {
            SetObjectVisibility(isVisible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }

    }

    private class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        Action updateCallBack;

        public LODMesh(int lod, Action updateCallBack)
        {
            this.lod = lod;
            this.updateCallBack = updateCallBack;
        }

        public void OnMeshHolderReceived(MeshHolder meshHolder)
        {
            mesh = meshHolder.GenerateMesh();
            hasMesh = true;

            updateCallBack();
        }

        public void RequestMesh(TerrainMapHolder terrainMap)
        {
            hasRequestedMesh = true;
            terrainMapGenerator.RequestMeshHolder(terrainMap, lod, OnMeshHolderReceived);
        }
    }

    [Serializable]
    public class LODInfo
    {
        [SerializeField] private int lod;
        [SerializeField] private float visibleDistanceLimit;
        [SerializeField] private bool useForCollider;

        public int Lod { get { return lod; } set { lod = value; } }
        public float VisibleDistanceLimit { get { return visibleDistanceLimit; } set { visibleDistanceLimit = value; } }

        public bool UseForCollider { get { return useForCollider; } }

    }
}

