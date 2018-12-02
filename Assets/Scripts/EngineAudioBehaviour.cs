using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudioBehaviour : MonoBehaviour
{
    // Car attached source
    public AudioSource CarEngine;

    // Engine sounds
    public EngineSample[] sounds;

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
        CarEngine.clip = sounds[lastIndex].clip;
        CarEngine.Play();
    }

    // Called from CarBehaviour, processes sounds
    public void ProcessSounds(float revs, bool spooled)
    {
        if (sounds.Length == 0) return;
        int currentIndex = lastIndex;

        // Which direction to go in the sound range
        if (sounds[currentIndex].lowRev > revs)
        {
            // Go down the sound range until revs fit (decelerating)
            while (sounds[currentIndex].lowRev > revs && lastIndex >= 0)
            {
                currentIndex--;
            }
        }
        else if (sounds[currentIndex].highRev < revs)
        {
            // Go down the sound range until revs fit (decelerating)
            while (sounds[currentIndex].highRev < revs && lastIndex < sounds.Length - 1)
            {
                currentIndex++;
            }
        }

        // At this point currentIndex points to the correct rev range
        // If it's a new sound, play it
        if (lastIndex != currentIndex)
        {
            CarEngine.clip = sounds[currentIndex].clip;
            CarEngine.pitch = 1f;
            CarEngine.Play();
            lastIndex = currentIndex;
        }

        // Apply the modifiers
        CarEngine.volume = sounds[currentIndex].relativeVolume;
        if (sounds[currentIndex].isPitchModified)
        {
            CarEngine.pitch = (revs / 1000) / 4;
        }

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
    public AudioClip clip;
    public float relativeVolume;
    // Maybe find a way to replace that with a way to parametrize the formula
    public bool isPitchModified;
}