using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{

    [SerializeField] private GameObject directionalLight;

    public GameObject DirectionalLight { get { return directionalLight; } }

    private static LightManager instance;
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
