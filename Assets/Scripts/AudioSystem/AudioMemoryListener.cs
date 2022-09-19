using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class AudioMemoryListener : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        EventSystem.Instance.RegisterListener<MemoryAudioEvent>(HandleAudioInfoEvent);
    }

    private void HandleAudioInfoEvent(MemoryAudioEvent ev)
    {
        audioSource.PlayOneShot(AudioLibrarySystem.Instance.SoundClip(ev.AudioClipType));
    }
}
