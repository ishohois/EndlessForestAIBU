using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationAssets : MonoBehaviour
{
    [SerializeField] private GameObject godRayParticles;
    [SerializeField] private GameObject fallingLeavesParticles;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material changeColorMaterial;
    [SerializeField] private MeshRenderer meshRenderer;
    public GameObject GodRayParticles { get { return godRayParticles; } }
    public GameObject FallingLeavesParticles { get { return fallingLeavesParticles; } }
    public Material DefaultMaterial { get { return defaultMaterial; } }
    public Material ChangeColorMaterial { get { return changeColorMaterial; } }
    public MeshRenderer MeshRenderer { get { return meshRenderer; } }


    private void OnDisable()
    {
        meshRenderer.material = defaultMaterial;

        if (fallingLeavesParticles != null)
            fallingLeavesParticles.SetActive(false);

        if (godRayParticles != null)
            godRayParticles.SetActive(false);
    }
}
