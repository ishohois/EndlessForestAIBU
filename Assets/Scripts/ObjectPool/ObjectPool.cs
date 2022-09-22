using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;
    
    private Dictionary<VegetationType, Queue<PooledObject>> objectPools = new Dictionary<VegetationType, Queue<PooledObject>>();
    private Dictionary<VegetationType, SpawnObject> spawnObjects = new Dictionary<VegetationType, SpawnObject>();
    private Queue<PooledObject> grassPool = new Queue<PooledObject>();

    [SerializeField] private List<SpawnObject> prefabs;
    [SerializeField] private SpawnObject grassPrefab;
    [SerializeField] private int totalNumberObjectPool;
    [SerializeField] private int totalNumberGrass;
    [SerializeField] private float totalPercentage;

    public List<SpawnObject> Prefabs { get { return prefabs; } }
    public Dictionary<VegetationType, SpawnObject> SpawnObjects { get { return spawnObjects; } }
    public Queue<PooledObject> GrassPool { get { return grassPool; } }
    public SpawnObject GrassPrefab { get { return grassPrefab; } }

    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectPool>();
            }
            return instance;
        }
    }

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnValidate()
    {
        float totalPercent = 0;

        foreach(SpawnObject spawnObject in prefabs)
        {
            totalPercent += spawnObject.PercentAmount;
        }

        totalPercentage = totalPercent * 100;
    }

    private void Awake()
    {
        foreach (SpawnObject spawnObject in prefabs)
        {
            int numberOfPoolObjects = (int)(spawnObject.PercentAmount * totalNumberObjectPool);

            if (objectPools.ContainsKey(spawnObject.ObjectType) == false)
            {
                objectPools.Add(spawnObject.ObjectType, new Queue<PooledObject>());
            }

            for (int i = 0; i < numberOfPoolObjects; i++)
            {
                GameObject gameObject = Instantiate(spawnObject.Prefab, transform);
                gameObject.transform.localScale *= Random.Range(spawnObject.MinScale, spawnObject.MaxScale);
                gameObject.SetActive(false);
                PooledObject pooledObject = new PooledObject { ObjectType = spawnObject.ObjectType, GameObject = gameObject, IsActive = false };
                objectPools[spawnObject.ObjectType].Enqueue(pooledObject);
            }
            spawnObjects.Add(spawnObject.ObjectType, spawnObject);
        }

        for (int i = 0; i < totalNumberGrass; i++)
        {
            GameObject gameObject = Instantiate(grassPrefab.Prefab, transform);
            gameObject.SetActive(false);
            PooledObject pooledObject = new PooledObject { ObjectType = grassPrefab.ObjectType, GameObject = gameObject, IsActive = false };
            grassPool.Enqueue(pooledObject);
        }

        EventSystem.Instance.RegisterListener<DespawnGrassEvent>(HandleDespawnGrassEvent);
    }

    public PooledObject SpawnGameObject(VegetationType objectType)
    {
        if (objectPools.ContainsKey(objectType) == false)
        {
            return null;
        }

        PooledObject pooledObject = objectPools[objectType].Dequeue();
        pooledObject.GameObject.SetActive(true);
        pooledObject.IsActive = true;
        objectPools[objectType].Enqueue(pooledObject);
        return pooledObject;
    }

    public void Despawn(PooledObject pooledObject)
    {
        objectPools[pooledObject.ObjectType].Enqueue(pooledObject);
        pooledObject.GameObject.transform.position = Vector3.zero;
        pooledObject.GameObject.SetActive(false);
        pooledObject.IsActive = false;
    }

    public PooledObject SpawnGrass()
    {
        PooledObject pooledObject = grassPool.Dequeue();
        pooledObject.GameObject.SetActive(true);
        pooledObject.IsActive = true;
        grassPool.Enqueue(pooledObject);
        return pooledObject;
    }

    private void HandleDespawnGrassEvent(DespawnGrassEvent ev)
    {
        foreach(PooledObject pooledObject in grassPool)
        {
            if(pooledObject.IsActive == false)
            {
                pooledObject.GameObject.transform.position = Vector3.zero;
                pooledObject.GameObject.SetActive(false);
            }
        }
    }

}

public class PooledObject
{
    public VegetationType ObjectType;
    public GameObject GameObject;
    public bool IsActive;
}
