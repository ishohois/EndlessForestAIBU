using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class AudioAmbientListener : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        EventSystem.Instance.RegisterListener<AmbientAudioEvent>(HandleAmbientAudioEventt);
    }

    private void HandleAmbientAudioEventt(AmbientAudioEvent ev)
    {
        Debug.Log("HandleAmbientAudioEventt " + ev.AudioClipType);
        audioSource.PlayOneShot(AudioLibrarySystem.Instance.SoundClip(ev.AudioClipType));
    }
}
