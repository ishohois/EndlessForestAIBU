using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

[RequireComponent(typeof(Collider))]
public class AudioTagObject : MonoBehaviour
{
    private bool hasPlayedAudioClip;
    private NarrativeAudioEvent narrativeInfoEvent;
    private float countTime;

    [SerializeField] private AudioClipType audioClipType;
    [SerializeField] private float resetTimeHasPlayedClip = 1f;

    public AudioClipType AudioClipType { get { return audioClipType; } }

    private void Start()
    {
        narrativeInfoEvent = new NarrativeAudioEvent { AudioClipType = audioClipType };
        countTime = resetTimeHasPlayedClip;
    }

    private void Update()
    {
        countTime -= Time.deltaTime;
        if (countTime <= 0)
        {
            countTime = resetTimeHasPlayedClip;
            hasPlayedAudioClip = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayedAudioClip == false)
        {
            EventSystem.Instance.FireEvent(narrativeInfoEvent);
            hasPlayedAudioClip = true;
        }
    }
}
