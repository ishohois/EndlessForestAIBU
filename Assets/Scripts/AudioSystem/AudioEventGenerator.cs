using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class AudioEventGenerator : MonoBehaviour
{
    private float counterTime;
    private EventCallbacksSystem.Event previousEvent;
    private bool hasFiredAudioEvent;

    [SerializeField] private float minRandomTime = 1f;
    [SerializeField] private float maxRandomTime = 15f;
    [SerializeField] List<AudioClipType> narrativeCliptypes;
    [SerializeField] List<AudioClipType> ambienceCliptypes;

    private List<BaseAudioEvent> audioEvents = new List<BaseAudioEvent>();

    private void Start()
    {
        audioEvents.Add(new NarrativeAudioEvent());
        audioEvents.Add(new AmbientAudioEvent());
      

        counterTime = Random.Range(minRandomTime, maxRandomTime);
    }

    private void Update()
    {
        counterTime -= Time.deltaTime;
        if(counterTime <= 0 && hasFiredAudioEvent == false)
        {
            hasFiredAudioEvent = true;
            counterTime = Random.Range(minRandomTime, maxRandomTime);
            FireAudioEvent();
        }
    }

    private void FireAudioEvent()
    {
        Debug.Log("AudioEventGenerator.FireAudioEvent()");

        // choose eventtype
        BaseAudioEvent audioEvent = audioEvents[Random.Range(0, audioEvents.Count)];
        AudioClipType audioClipType = AudioClipType.BABYTOY;
        
        if(previousEvent != null && previousEvent.GetType() == typeof(NarrativeAudioEvent))
        {
            audioEvent = audioEvents[Random.Range(0, audioEvents.Count)];
        }

        // choose audioclip
        if(audioEvent.GetType() == typeof(NarrativeAudioEvent))
        {
            audioClipType = narrativeCliptypes[Random.Range(0, narrativeCliptypes.Count)];
            audioEvent.AudioClipType = audioClipType;
        }

        else if (audioEvent.GetType() == typeof(AmbientAudioEvent))
        {
            audioClipType = ambienceCliptypes[Random.Range(0, ambienceCliptypes.Count)];
            audioEvent.AudioClipType = audioClipType;
        }

        // fire event
        EventSystem.Instance.FireEvent(audioEvent);
        previousEvent = audioEvent;
        hasFiredAudioEvent = false;
    }

}
