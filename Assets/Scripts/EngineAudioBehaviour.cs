using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudioBehaviour : MonoBehaviour
{
    // Car attached sources

    // Engine sounds
    public EngineSample[] SoundArray;


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
        SoundArray[0].sample = SoundArray[lastIndex].sample;
        SoundArray[0].output.Play();
    }

    // Called from CarBehaviour, processes sounds
    public void ProcessSounds(float revs, bool spooled)
    {
        if (SoundArray.Length == 0) return;
        int currentIndex = lastIndex;

        // Which direction to go in the sound range
        if (SoundArray[currentIndex].lowRev > revs)
        {
            // Go down the sound range until revs fit (decelerating)
            while (SoundArray[currentIndex].lowRev > revs && lastIndex >= 0)
            {
                currentIndex--;
            }
        }
        else if (SoundArray[currentIndex].highRev < revs)
        {
            // Go down the sound range until revs fit (decelerating)
            while (SoundArray[currentIndex].highRev < revs && lastIndex < SoundArray.Length - 1)
            {
                currentIndex++;
            }
        }
        // At this point currentIndex points to the correct rev range
        // If it's a new sound, play it
        if (lastIndex != currentIndex)
        {
            SoundArray[0].sample = SoundArray[currentIndex].sample;
            SoundArray[0].output.volume = 1f;
            SoundArray[0].output.Play();
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
    public int lowRev;//engagement RPM
    public int highRev;//disengagement RPM
    public AudioClip sample;//sample to play
    public AudioSource output;//output audio source. first,second,first,second..
    public AnimationCurve volumeCurve;//volume curve. starts from 0 - 100. eval by mathf.lerp(lowrev - highrev)
    public AnimationCurve pitchCurve;//pitch curve. same as volume curve pretty much
}