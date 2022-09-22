using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Vegetation/vegetation")]
public class SpawnObject : ScriptableObject
{
    [SerializeField] private VegetationType objectType;
    [SerializeField] private GameObject prefab;
    [Range(0, 1)]
    [SerializeField] private float percentAmount;
    [Range(0.5f, 1)]
    [SerializeField] private float minScale;
    [Range(1, 3)]
    [SerializeField] private float maxScale;

    [SerializeField] private float minSpawnHeightLimit;
    [SerializeField] private float maxSpawnHeightLimit;

    [Range(0, 1)]
    [SerializeField] private float thresholdGodrays;
    [Range(0, 1)]
    [SerializeField] private float thresholdFallingLeaves;
    [Range(0, 1)]
    [SerializeField] private float thresholdGrassPatches;

    public VegetationType ObjectType { get { return objectType; } }
    public GameObject Prefab { get { return prefab; } }
    public float PercentAmount { get { return percentAmount; } }
    public float MinScale { get { return minScale; } }
    public float MaxScale { get { return maxScale; } }
    public float MinSpawnHeightLimit { get { return minSpawnHeightLimit; } }
    public float MaxSpawnHeightLimit { get { return maxSpawnHeightLimit; } }
    public float ThresholdGodrays { get { return thresholdGodrays; } }
    public float ThresholdFallingLeaves { get { return thresholdFallingLeaves; } }
    public float ThresholdGrassPatches { get { return thresholdGrassPatches; } }
    
}

public enum VegetationType
{
    Default,
    Tree1,
    Tree1_wDetail,
    Tree2,
    Tree2_wDetail,
    Tree3,
    Tree3_wDetail,
    Tree4,
    Tree4_wDetail,
    Bush1,
    Bush2,
    Mushroom,
    Log1,
    Log1_grownOver,
    Log2,
    Log2_logOnRock,
    Rock1,
    Rock1_wDetail,
    Rock2,
    Rock2_wDetail,
    Grass,
    Grass_patch,
    Flower1,
    Flower2,
    Trunk,
    Trunk_wDetail,
    Chonky
}