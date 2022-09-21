using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationTag : MonoBehaviour
{

    [SerializeField] private GameObject godRayParticles;
    [SerializeField] private GameObject fallingLeavesParticles;
    [SerializeField] private GameObject chonkyBoy;

    public GameObject GodRayParticles { get { return godRayParticles; } }
    public GameObject FallingLeavesParticles { get { return fallingLeavesParticles; } }
    public GameObject ChonkyBoy { get { return chonkyBoy; } }
}
