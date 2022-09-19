using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AudioClipConfig
{
    [SerializeField] private AudioClipType audioClipType;
    [SerializeField] private AudioClip audioClip;

    public AudioClipType AudioClipType { get { return audioClipType; } }
    public AudioClip AudioClip { get { return audioClip; } }
}
