using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryObject : MonoBehaviour
{
    private bool hasChangedVegetationColor;

    [SerializeField] private float radius = 1f;
    [SerializeField] private LayerMask colorChangeLayers;
    [SerializeField] private SphereCollider sphereCollider;

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

    private void Update()
    {
        if (hasChangedVegetationColor == false)
        {
            ChangeColorOfVegetation();
        }
    }

}
