using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteReader : MonoBehaviour {

    public AudioClip sound;
    public AudioSource soundOutput;
    public bool played = false;
    public CanvasGroup noteCanvas;
    public Image displayNote;
    public Sprite note;
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}
    private void OnTriggerStay(Collider other)
    {
        if (!played)
        {
            displayNote.sprite = note;
            StartCoroutine(PrintNote());
            soundOutput.clip = sound;
            soundOutput.Play();
            played = true;
        }

    }
    public IEnumerator PrintNote()
    {

        noteCanvas.alpha = 1;
        yield return new WaitForSeconds(0.5f);
        noteCanvas.alpha = 0;
        yield return new WaitForSeconds(0.5f);
        noteCanvas.alpha = 1;
        yield return new WaitForSeconds(0.3f);
        noteCanvas.alpha = 0;
        yield return new WaitForSeconds(0.3f);
    }

}
