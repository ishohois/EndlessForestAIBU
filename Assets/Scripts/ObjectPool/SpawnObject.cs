using System;
using UnityEngine;

[Serializable]
public class SpawnObject
{
    [SerializeField] private ObjectType objectType;
    [SerializeField] private GameObject prefab;
    [Range(0, 1)]
    [SerializeField] private float percentAmount;
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;
    [SerializeField] private float minSpawnHeightLimit;
    [SerializeField] private float maxSpawnHeightLimit;
    [SerializeField] private float percentToHaveGodRays;
    [SerializeField] private float percentToHaveFallingLeaves;
    [SerializeField] private float percentToHaveChonky;

    public ObjectType ObjectType { get { return objectType; } }
    public GameObject Prefab { get { return prefab; } }
    public float PercentAmount { get { return percentAmount; } }
    public float MinScale { get { return minScale; } }
    public float MaxScale { get { return maxScale; } }
    public float MinSpawnHeightLimit { get { return minSpawnHeightLimit; } }
    public float MaxSpawnHeightLimit { get { return maxSpawnHeightLimit; } }
    public float PercentToHaveGodRays { get { return percentToHaveGodRays; } }
    public float PercentToHaveFallingLeaves { get { return percentToHaveFallingLeaves; } }
    public float PercentToHaveChonky { get { return percentToHaveChonky; } }
}

public enum ObjectType
{
    Default,
    Tree1,
    Tree2,
    Tree3,
    Tree4,
    Bush1,
    Bush2,
    Mushroom,
    Log1,
    Log2,
    Rock1,
    Rock2,
    Grass,
    Flower1,
    Flower2,
    Trunk,
    Chonky

}