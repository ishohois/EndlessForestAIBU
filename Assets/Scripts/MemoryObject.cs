using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class MemoryObject : MonoBehaviour
{
    private bool hasChangedVegetationColor;
    private bool hasBeenCollected;
    private bool isActivated;

    [SerializeField] private float radius = 1f;
    [SerializeField] private LayerMask colorChangeLayers;
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private MemoryObjectType memoryObjectType;
    [SerializeField] private float speedMultiplier = 2f;

    public MemoryObjectType MemoryObjectType { get { return memoryObjectType; } }

    private void ChangeColorOfVegetation()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, colorChangeLayers);

        if (colliders.Length > 0)
        {
            foreach (Collider coll in colliders)
            {
                VegetationAssets vegetation = coll.transform.gameObject.GetComponent<VegetationAssets>();
                if (vegetation != null)
                {
                    vegetation.MeshRenderer.material = vegetation.ChangeColorMaterial;
                }
            }

            hasChangedVegetationColor = true;
        }

        sphereCollider.enabled = false;
    }

    private void OnEnable()
    {
        isActivated = true;
    }

    private void Update()
    {
        if (hasChangedVegetationColor == false && isActivated == true)
        {
            ChangeColorOfVegetation();
        }

        if(hasBeenCollected == true)
        {
            transform.position += Time.deltaTime * Vector3.up * speedMultiplier;
        }

        if(transform.position.y >= 50f)
        {
            transform.position = Vector3.zero;
            gameObject.SetActive(false);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        hasBeenCollected = true;
        
        if(hasBeenCollected == true)
        {
            EventSystem.Instance.FireEvent(new MemoryCollectedEvent { MemoryObjectType = memoryObjectType });
        }
    }
}

public enum MemoryObjectType
{
    Default,
    BASEBALLTROPHY,
    WEDDINGPICTURE,
    BABYTOY,
    BASEBALLBAT,
    FAMILYPORTRAIT,

}
