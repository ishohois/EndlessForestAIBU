using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Footsteps : MonoBehaviour
{
    public AudioSource audioSource;
    //[SerializeField] AudioClip[] audioClip;
   // public PhysicsBody controller;
    // Start is called before the first frame update
    
     void Update() {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)){
            audioSource.enabled = true;
        } 
        else{
            audioSource.enabled = false;
        }
    }
}
