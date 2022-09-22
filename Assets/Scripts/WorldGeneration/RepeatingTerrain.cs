using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class RepeatingTerrain : MonoBehaviour
{
    private const float ViewerMovementChunkUpdateLimit = 25f;
    private const float sqrViewerMovementChunkUpdateLimit = ViewerMovementChunkUpdateLimit * ViewerMovementChunkUpdateLimit;

    private static float maxViewDistance = 250f;
    private static float activationDistance = 400f;
    private static TerrainMapGenerator terrainMapGenerator;
    private static List<TerrainChunk> chunksWithObjects = new List<TerrainChunk>();
    private List<TerrainChunk> chunksToDelete = new List<TerrainChunk>();
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

        foreach(var chunk in chunksWithObjects)
        {
            chunk.UpdateChunk();
            if(chunk.DeactivateChunk == true)
            {
                chunk.DestroyChunk();
                chunksToDelete.Add(chunk);
            }
        }

        foreach(var chunk in chunksToDelete)
        {
            chunksWithObjects.Remove(chunk);
            terrainChunks.Remove(chunk.keyPosition);
        }

    }

    private class TerrainChunk
    {
        public GameObject meshObject;
        public Vector2 position;
        public Vector2 keyPosition;
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
        List<PooledObject> grassPatchObjects = new List<PooledObject>();
        public bool DeactivateChunk;
        private DespawnGrassEvent grassEvent = new DespawnGrassEvent();

        public TerrainChunk(Vector2 viewerPostion, int size, float scale, Transform parent, Material material, List<SpawnObject> vegetationPrefabs)
        {
            this.scale = scale;
            this.size = size;
            this.prefabs = vegetationPrefabs;

            keyPosition = viewerPostion;
            position = viewerPostion * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 vector3 = new Vector3(position.x, 0f, position.y);

            meshObject = new GameObject("Terrain chunk" + generatedChunks);
            meshObject.isStatic = true;
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
                DeactivateChunk = viewerDistanceFromNearestEdge > activationDistance;
                if (hasPlacedPositions == true && hasPlacedObjects == false)
                {
                    PlaceObjects();
                }

                SetObjectVisibility(visible);
            }
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
            chunksWithObjects.Add(this);
            float chunkSize = (float)(size * scale);
            Vector3 startPosSpawn = new Vector3(
               meshObject.transform.position.x - (chunkSize / 2),
               60f,
               meshObject.transform.position.z + (chunkSize / 2));
            Vector3 posToSpawn = startPosSpawn;


            // place grass patches
            SpawnObject grassPrefab = ObjectPool.Instance.GrassPrefab;
            int grassPatches = Mathf.CeilToInt(pointsOfObjectsToBePlaced.Count * grassPrefab.PercentAmount);

            for (int i = 0; i < grassPatches; i++)
            {
                posToSpawn.x += pointsOfObjectsToBePlaced[i].x;
                posToSpawn.z += pointsOfObjectsToBePlaced[i].y - chunkSize;


                Ray ray = new Ray(posToSpawn, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.point.y > grassPrefab.MaxSpawnHeightLimit || hit.point.y < grassPrefab.MinSpawnHeightLimit)
                    {
                        posToSpawn = startPosSpawn;
                        continue;
                    }

                    PooledObject pooledObject = ObjectPool.Instance.SpawnGrass();
                    pooledObject.GameObject.transform.position = hit.point - Vector3.up;
                    pooledObject.GameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    grassPatchObjects.Add(pooledObject);
                }

                posToSpawn = startPosSpawn;
            }


            foreach (Point point in pointsOfObjectsToBePlaced)
            {
                if (point.objectType != VegetationType.Default)
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

                        if (hit.point.y > spawnObject.MaxSpawnHeightLimit || hit.point.y < spawnObject.MinSpawnHeightLimit)
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

                        if (spawnObject.VegetationSettings == VegetationSettings.GODRAY || spawnObject.VegetationSettings == VegetationSettings.FALLINGLEAVESGODRAY)
                        {
                            Debug.Log("Vegetation type" + spawnObject.ObjectType);
                            float chanceToSpawnGodRays = ObjectPlacement.RandomValue();
                            if (chanceToSpawnGodRays >= spawnObject.ThresholdGodrays)
                            {
                                objectToPlace.GetComponent<VegetationAssets>().GodRayParticles.SetActive(true);
                                Vector3 eulerAngles = LightManager.Instance.DirectionalLight.transform.rotation.eulerAngles;
                                eulerAngles.x = -eulerAngles.x;
                                objectToPlace.GetComponent<VegetationAssets>().GodRayParticles.transform.rotation = Quaternion.Euler(Vector3.zero);
                                objectToPlace.GetComponent<VegetationAssets>().GodRayParticles.transform.localRotation = Quaternion.Euler(Vector3.zero);
                                objectToPlace.GetComponent<VegetationAssets>().GodRayParticles.transform.rotation = Quaternion.Euler(eulerAngles);
                            }
                        }

                        if (spawnObject.VegetationSettings == VegetationSettings.FALLINGLEAVES || spawnObject.VegetationSettings == VegetationSettings.FALLINGLEAVESGODRAY)
                        {
                            float chanceToHaveFallingLeaves = ObjectPlacement.RandomValue();
                            if (chanceToHaveFallingLeaves >= spawnObject.ThresholdFallingLeaves)
                            {
                                objectToPlace.GetComponent<VegetationAssets>().FallingLeavesParticles.SetActive(true);
                            }
                        }
                    }
                }
                posToSpawn = startPosSpawn;
            }
        }

        public void DespawnObjects()
        {
            foreach (PooledObject pooledObject in pooledObjects)
            {
                ObjectPool.Instance.Despawn(pooledObject);
            }
            foreach(PooledObject pooled in grassPatchObjects)
            {
                pooled.IsActive = false;
            }

            EventSystem.Instance.FireEvent(grassEvent);
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

