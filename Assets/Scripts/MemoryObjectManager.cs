using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class MemoryObjectManager : MonoBehaviour
{
    private static MemoryObjectManager instance;

    private Dictionary<MemoryObjectType, bool> memoriesCollected = new Dictionary<MemoryObjectType, bool>();
    private int index;

    [SerializeField] private List<GameObject> memoryObjectsPrefabs;

    public Dictionary<MemoryObjectType, bool> MemoriesRetreived { get { return memoriesCollected; } }


    public static MemoryObjectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MemoryObjectManager>();
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

        foreach (GameObject memoryObject in memoryObjectsPrefabs)
        {
            memoriesCollected.Add(memoryObject.GetComponent<MemoryObject>().MemoryObjectType, false);
        }
    }

    private void Awake()
    {
        EventSystem.Instance.RegisterListener<MemoryCollectedEvent>(HandleMemoryCollectedEvent);
    }

    private void HandleMemoryCollectedEvent(MemoryCollectedEvent ev)
    {
        memoriesCollected[ev.MemoryObjectType] = true;
        bool areMemoriesCollected = false;
        foreach (var memory in memoriesCollected)
        {
            areMemoriesCollected = memory.Value;
            if (areMemoriesCollected == false)
            {
                break;
            }
        }

        if (areMemoriesCollected == true)
        {
            // roll credits;,
            MenuScript.LoadScene("Credit");
        }
    }

    public Vector3 NextPositionOfMemory(Vector3 startPos)
    {
        foreach(var item in memoriesCollected)
        {

        }
        return Vector3.zero;
    }

    public GameObject SpawnMemoryObject()
    {
        if (index < memoryObjectsPrefabs.Count)
        {
            GameObject gameObject = Instantiate(memoryObjectsPrefabs[index]);
            gameObject.transform.parent = transform;
            gameObject.SetActive(false);
            index++;
            return gameObject;
        }
        else
        {
            return null;
        }
    }
}
