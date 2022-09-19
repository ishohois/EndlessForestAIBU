using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class AudioNarrativeListener : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        EventSystem.Instance.RegisterListener<NarrativeAudioEvent>(HandleAudioInfoEvent);
    }

    private void HandleAudioInfoEvent(NarrativeAudioEvent ev)
    {
        audioSource.PlayOneShot(AudioLibrarySystem.Instance.SoundClip(ev.AudioClipType));
    }

}

