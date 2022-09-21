using System.Collections.Generic;
using UnityEngine;

public class RepeatingTerrain : MonoBehaviour
{
    private const float ViewerMovementChunkUpdateLimit = 25f;
    private const float sqrViewerMovementChunkUpdateLimit = ViewerMovementChunkUpdateLimit * ViewerMovementChunkUpdateLimit;
    private const int MaxNumberOfChunks = 30;

    private static float maxViewDistance = 250f;
    private static TerrainMapGenerator terrainMapGenerator;
    private static List<TerrainChunk> lastActiveChunks = new List<TerrainChunk>();
    private static int generatedChunks = 1;

    private int chunkSize;
    private int chunkVisibleInViewDistance;
    private Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    private Vector2 viewerPositionOld;

    [SerializeField] private Material mapMaterial;

    public static Vector2 viewerPosition;


    public Transform viewer;

    private void Start()
    {
        terrainMapGenerator = FindObjectOfType<TerrainMapGenerator>();
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

    private void UpdateVisibleChunks()
    {
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
                    terrainChunks.Add(chunkInViewPositiom, new TerrainChunk(chunkInViewPositiom, chunkSize, terrainMapGenerator.TerrainInfo.UniformScale, transform, mapMaterial, ObjectPool.Instance.Prefabs));
                }
            }
        }

        foreach (var chunk in terrainChunks)
        {
            chunk.Value.UpdateChunk();
            if (chunk.Value.IsVisible() == false)
            {
                chunk.Value.DespawnObjects();
            }
        }

        //if (generatedChunks >= MaxNumberOfChunks)
        //{
        //    foreach (var chunk in lastActiveChunks)
        //    {
        //        if (chunk.IsVisible() == false)
        //        {
        //            chunk.DestroyChunk();
        //            terrainChunks.Remove(chunk.keyPosition);
        //        }
        //    }

        //    generatedChunks -= (generatedChunks % MaxNumberOfChunks);
        //}

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
        List<SpawnObject> prefabs;
        bool hasPlacedPositions;
        bool hasPlacedObjects;
        List<Point> pointsOfObjectsToBePlaced = new List<Point>();
        List<PooledObject> pooledObjects = new List<PooledObject>();

        public TerrainChunk(Vector2 viewerPostion, int size, float scale, Transform parent, Material material, List<SpawnObject> vegetationPrefabs)
        {
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
            meshObject.transform.localScale = Vector3.one * terrainMapGenerator.TerrainInfo.UniformScale;

            terrainMapGenerator.RequestTerrainMapHolder(position, OnTerrainMapHolderReceived);

            generatedChunks++;
        }

        private void OnTerrainMapHolderReceived(TerrainMapHolder terrainMapHolder)
        {
            this.terrainMap = terrainMapHolder;
            terrainMapReceived = true;
            terrainMapGenerator.RequestMeshHolder(terrainMap, 0, OnMeshHolderReceived);
        }

        private void OnMeshHolderReceived(MeshHolder meshHolder)
        {
            meshFilter.mesh = meshHolder.GenerateMesh();
            PlacePositions(size * scale, meshObject.transform, prefabs);
            UpdateChunk();
        }

        private void SetObjectVisibility(bool isVisible)
        {
            isChunkVisible = isVisible;
            if (meshObject != null)
                meshObject.SetActive(isVisible);
        }

        public void UpdateChunk()
        {
            if (terrainMapReceived == true)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge < maxViewDistance;
                if (hasPlacedPositions == true && hasPlacedObjects == false)
                {
                    PlaceObjects();
                }
                //else if (hasPlacedPositions == true && hasPlacedObjects == true)
                //{
                //    DespawnObjects();
                //}
                SetObjectVisibility(visible);
            }

            Debug.Log("Viewer " + viewerPosition + "visible " + isChunkVisible);
        }

        private void PlacePositions(float chunkSize, Transform chunkTransform, List<SpawnObject> prefabs)
        {
            meshCollider = meshObject.AddComponent<MeshCollider>();
            hasPlacedPositions = true;
            List<Point> points = ObjectPlacement.GeneratePoints(new Vector2(chunkSize, chunkSize), 15f, 30);
            Vector3 startPosSpawn = new Vector3(
                chunkTransform.transform.position.x - ((chunkSize) / 2),
                60f,
                chunkTransform.transform.position.z + ((chunkSize) / 2));
            Vector3 posToSpawn = startPosSpawn;

            foreach (SpawnObject spawnObject in ObjectPool.Instance.Prefabs)
            {
                int numberOfIterations = (int)(spawnObject.PercentAmount * points.Count);

                for (int i = 0; i < numberOfIterations; i++)
                {
                    if (points[i].isPositionTaken == false)
                    {
                        posToSpawn.x += points[i].x;
                        posToSpawn.z += points[i].y - chunkSize;

                        Ray ray = new Ray(posToSpawn, Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit hit))
                        {
                            if (hit.point.y > spawnObject.MaxSpawnHeightLimit || hit.point.y < spawnObject.MinSpawnHeightLimit)
                            {
                                posToSpawn = startPosSpawn;
                                continue;
                            }
                            else
                            {
                                points[i].isPositionTaken = true;
                                points[i].objectType = spawnObject.ObjectType;
                            }
                        }
                    }
                    posToSpawn = startPosSpawn;
                }
            }

            foreach (Point point in points)
            {
                SpawnObject randomSpawn = ObjectPool.Instance.Prefabs[ObjectPlacement.RandomBetweenRangeInt(0, prefabs.Count)];
                if (point.isPositionTaken == false)
                {
                    posToSpawn.x += point.x;
                    posToSpawn.z += point.y - chunkSize;

                    Ray ray = new Ray(posToSpawn, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (hit.point.y > randomSpawn.MaxSpawnHeightLimit || hit.point.y < randomSpawn.MinSpawnHeightLimit)
                        {
                            posToSpawn = startPosSpawn;
                            continue;
                        }
                        else
                        {
                            point.isPositionTaken = true;
                            point.objectType = randomSpawn.ObjectType;
                        }
                    }
                }
                posToSpawn = startPosSpawn;
            }

            pointsOfObjectsToBePlaced = points;
            Debug.Log(points.Count);
        }

        private void PlaceObjects()
        {
            hasPlacedObjects = true;
            float chunkSize = (float)(size * scale);
            Vector3 startPosSpawn = new Vector3(
               meshObject.transform.position.x - (chunkSize / 2),
               60f,
               meshObject.transform.position.z + (chunkSize / 2));
            Vector3 posToSpawn = startPosSpawn;

            foreach (Point point in pointsOfObjectsToBePlaced)
            {
                if (point.objectType != ObjectType.Default)
                {
                    SpawnObject spawnObject = ObjectPool.Instance.SpawnObjects[point.objectType];

                    posToSpawn.x += point.x;
                    posToSpawn.z += point.y - chunkSize;

                    Ray ray = new Ray(posToSpawn, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {

                        if (hit.transform.gameObject.GetComponent<VegetationTag>() == true)
                        {
                            posToSpawn = startPosSpawn;
                            continue;
                        }

                        PooledObject pooledObject = ObjectPool.Instance.SpawnGameObject(spawnObject.ObjectType);
                        pooledObjects.Add(pooledObject);
                        GameObject objectToPlace = pooledObject.GameObject;
                        objectToPlace.transform.position = hit.point - Vector3.up;
                        objectToPlace.transform.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, 0), Quaternion.FromToRotation(Vector3.up, hit.normal), 0.5f);
                        objectToPlace.transform.Rotate(Vector3.up, ObjectPlacement.RandomBetweenRange(0, 360));
                    }
                }
                posToSpawn = startPosSpawn;
            }
        }

        public void DespawnObjects()
        {
            //Debug.Log("TerrainChunk despawned objects" + meshObject.name);
            foreach (PooledObject pooledObject in pooledObjects)
            {
                ObjectPool.Instance.Despawn(pooledObject);
            }
            pooledObjects.Clear();
            hasPlacedObjects = false;
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
            DespawnObjects();
            Destroy(meshObject);
        }
    }

}

