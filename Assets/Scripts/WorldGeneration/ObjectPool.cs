using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;

    [SerializeField] private List<SpawnObject> prefabs;
    [SerializeField] private int totalNumberObjectPool;
    Dictionary<ObjectType, Queue<PooledObject>> objectPools = new Dictionary<ObjectType, Queue<PooledObject>>();
    Dictionary<ObjectType, SpawnObject> spawnObjects = new Dictionary<ObjectType, SpawnObject>();

    public List<SpawnObject> Prefabs { get { return prefabs; } }
    public Dictionary<ObjectType, SpawnObject> SpawnObjects { get { return spawnObjects; } }

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

    private void Awake()
    {
        // Setting up the object pools
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
    }

    public PooledObject SpawnGameObject(ObjectType objectType)
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
        pooledObject.GameObject.transform.localScale = Vector3.one;
        pooledObject.GameObject.SetActive(false);
        pooledObject.IsActive = false;
    }

}
public class PooledObject
{
    public ObjectType ObjectType;
    public GameObject GameObject;
    public bool IsActive;
}
