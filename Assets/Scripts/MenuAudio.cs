using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudio : MonoBehaviour {
    public AudioSource MenuAudioSource;

	void Start () {
        SetVolume();
        MenuAudioSource.Play();
    }

    public void SetVolume() {
        float menuAudio = FindObjectOfType<DataController>().MenuAudio;
        MenuAudioSource.volume = menuAudio;
    }
}
