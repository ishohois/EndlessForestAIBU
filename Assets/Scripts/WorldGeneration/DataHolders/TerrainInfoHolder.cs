using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

[CreateAssetMenu()]
public class TerrainInfoHolder : UpdatableInfo
{
    private UpdatedTerrainInfoEvent updatedTerrainInfoEvent;

    [SerializeField] private bool useFallOff;
    [SerializeField] private bool useFlatShading;
    [SerializeField] private float uniformScale = 2.5f;
    [SerializeField] private float heightMultiplier;
    [SerializeField] private AnimationCurve heightCurve;

    protected override void OnValidate()
    {
        if (updatedTerrainInfoEvent == null)
        {
            updatedTerrainInfoEvent = new UpdatedTerrainInfoEvent();
            infoEvent = updatedTerrainInfoEvent;
        }

        base.OnValidate();
    }

    public bool UseFallOff { get { return useFallOff; } }
    public bool UseFlatShading { get { return useFlatShading; } }
    public float UniformScale { get { return uniformScale; } }
    public float HeightMultiplier { get { return heightMultiplier; } }
    public AnimationCurve HeightCurve { get { return heightCurve; } }

}
