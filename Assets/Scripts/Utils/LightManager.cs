using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    private static LightManager instance;
    
    [SerializeField] private GameObject directionalLight;

    public GameObject DirectionalLight { get { return directionalLight; } }

    public static LightManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LightManager>();
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

}
