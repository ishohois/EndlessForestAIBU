using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationAssets : MonoBehaviour
{
    [SerializeField] private GameObject godRayParticles;
    [SerializeField] private GameObject fallingLeavesParticles;
    public GameObject GodRayParticles { get { return godRayParticles; } }
    public GameObject FallingLeavesParticles { get { return fallingLeavesParticles; } }
}
