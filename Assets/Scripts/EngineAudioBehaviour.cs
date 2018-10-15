using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudioBehaviour : MonoBehaviour
{
    // Car attached sources
    public AudioSource CarEngine1;
    public AudioSource CarEngine2;

    // Engine sounds
    public EngineSample[] layer0;
    public EngineSample[] layer1;


    // Turbo related stuff
    public bool hasTurbo;
    public AudioSource boostSource;
    public AudioClip woosh;
    public AudioClip pssh;
    public bool isSpooled;

    // Some attempt at blindly speeding up processing
    private int lastIndex = 0;

    // Initialize audio
    void Start()
    {
        CarEngine1.clip = layer0[lastIndex].sample;
        CarEngine1.Play();
    }

    // Called from CarBehaviour, processes sounds
    public void ProcessSounds(float revs, bool spooled)
    {
        if (layer0.Length == 0) return;
        int currentIndex = lastIndex;

        // Which direction to go in the sound range
        if (layer0[currentIndex].lowRev > revs)
        {
            // Go down the sound range until revs fit (decelerating)
            while (layer0[currentIndex].lowRev > revs && lastIndex >= 0)
            {
                currentIndex--;
            }
        }
        else if (layer0[currentIndex].highRev < revs)
        {
            // Go down the sound range until revs fit (decelerating)
            while (layer0[currentIndex].highRev < revs && lastIndex < layer0.Length - 1)
            {
                currentIndex++;
            }
        }
        // At this point currentIndex points to the correct rev range
        // If it's a new sound, play it
        if (lastIndex != currentIndex)
        {
            CarEngine1.clip = layer0[currentIndex].sample;
            CarEngine1.pitch = 1f;
            CarEngine1.Play();
            lastIndex = currentIndex;
        }

        // Apply the modifiers
        //CarEngine1.volume = sounds[currentIndex].relativeVolume; APPLIES VOLUME
        //if (sounds[currentIndex].isPitchModified)
        //{
            //CarEngine1.pitch = (revs / 1000) / 4;
        //}

        // Process turbo sound
        if (hasTurbo)
        {
            // turbo started spooling
            if (!isSpooled && spooled)
            {
                boostSource.clip = woosh;
                boostSource.loop = true;
                boostSource.Play();
                isSpooled = spooled;
            }

            // turbo stopped spooling
            if (isSpooled && !spooled)
            {
                boostSource.clip = pssh;
                boostSource.loop = false;
                boostSource.Play();
                isSpooled = spooled;
            }
        }
    }
}

[System.Serializable]
public struct EngineSample
{
    public int lowRev;
    public int highRev;
    public AudioClip sample;
    public AnimationCurve volumeCurve;
    public AnimationCurve pitchCurve;
}