using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLibrarySystem : MonoBehaviour
{
    private static AudioLibrarySystem instance;

    private Dictionary<AudioClipType, AudioClip> soundClips;

    [Tooltip("Add the configs that are to be used for object")]
    [SerializeField] private List<AudioClipConfig> audioClips;

    public static AudioLibrarySystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioLibrarySystem>();
            }
            return instance;
        }
    }

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (soundClips == null)
        {
            soundClips = new Dictionary<AudioClipType, AudioClip>();
        }

        if (audioClips != null)
        {
            foreach (AudioClipConfig clip in audioClips)
            {
                if (!soundClips.ContainsKey(clip.AudioClipType))
                {
                    soundClips.Add(clip.AudioClipType, null);
                }
                soundClips[clip.AudioClipType] = clip.AudioClip;
            }
        }

    }

    public AudioClip SoundClip(AudioClipType clipType)
    {
        if (soundClips.ContainsKey(clipType))
        {
            return soundClips[clipType];
        }
        return null;
    }

}
