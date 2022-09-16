using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

[CreateAssetMenu()]
public class NoiseInfoHolder : UpdatableInfo
{
    private UpdatedNoiseInfoEvent updatedNoiseInfoEvent;
    
    [SerializeField] private NoiseNormalizationMode noiseNormalization;
    [SerializeField] private float noiseNormaliseFactor;
    [SerializeField] private float noiseScale;
    [SerializeField] private int octaves;
    [Range(0, 1)]
    [SerializeField] private float persistence;
    [SerializeField] private float lacunarity;
    [SerializeField] private Vector2 offset;

    [SerializeField] private int seed;
    [SerializeField] private bool randomSeed;


    public NoiseNormalizationMode NoiseNormalization { get { return noiseNormalization; } }
    public float NoiseNormaliseFactor { get { return noiseNormaliseFactor; } }
    public float NoisseScale { get { return noiseScale; } }
    public int Octaves { get { return octaves; } }
    public float Persistence { get { return persistence; } }
    public float Lacunarity { get { return lacunarity; } }
    public Vector2 Offset { get { return offset; } }

    public int Seed
    {
        get
        {
            if (randomSeed)
            {
                System.Random random = new System.Random();
                seed = random.Next(-1000000, 1000000);
            }
            return seed;
        }
        set { seed = value; }
    }

    protected override void OnValidate()
    {

        if(updatedNoiseInfoEvent == null)
        {
            updatedNoiseInfoEvent = new UpdatedNoiseInfoEvent();
            infoEvent = updatedNoiseInfoEvent;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        base.OnValidate();
    }
}
