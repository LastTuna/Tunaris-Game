using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudioBehaviour : MonoBehaviour
{
    // Car attached source
    public AudioSource[] CarEngine;
    public AudioSource horn;
    //can essentially have as many sound layers as you want


    // Engine sounds
    public EngineSampleArray[] sounds;
    //MULTIDIMENSIONAL ARRAY
    //FIRST COMES INDEX
    //SECOND COMES LAYER
    public int[] rpmIndex;
    //index the sounds by rpm range for quickly finding the relevant sound samples

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
        CarEngine[0].clip = sounds[lastIndex].index[0].clip;
        CarEngine[0].Play();
    }

    // Called from CarBehaviour, processes sounds
    public void ProcessSounds(float revs, bool spooled, float boostPressure)
    {
        if (sounds.Length == 0) return;//null check

        //find index
        int currentIndex = SoundIndex(revs);
        // At this point currentIndex points to the correct rev range

        // If it's a new sound, play it
        if (lastIndex != currentIndex)
        {
            //go through all layers in current index and apply samples. mute unused layers
            int i = 0;
            for (i = 0; i < sounds[currentIndex].index.Length; i++)
            {
                CarEngine[i].clip = sounds[currentIndex].index[i].clip;
                CarEngine[i].pitch = 1f;
                CarEngine[i].Play();
            }
            while (i < CarEngine.Length)
            {
                CarEngine[i].clip = null;
                i++;
            }
            lastIndex = currentIndex;
        }
        // Apply the modifiers
        for (int i = 0; i < sounds[currentIndex].index.Length; i++)
        {
            float factor = Mathf.InverseLerp(sounds[currentIndex].index[i].lowRev, sounds[currentIndex].index[i].highRev, revs);
            CarEngine[i].pitch = sounds[currentIndex].index[i].audioPitch.Evaluate(factor);
            CarEngine[i].volume = sounds[currentIndex].index[i].audioVolume.Evaluate(factor);
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
                boostSource.pitch = 1;
                boostSource.clip = pssh;
                boostSource.loop = false;
                boostSource.Play();
                isSpooled = spooled;
            }
        }
    }

    public int SoundIndex(float revs)
    {
        int rpm = (int)revs;
        for (int i = 0; i < rpmIndex.Length; i++)
        {
            if (rpm > rpmIndex[i] && rpm < rpmIndex[i + 1])
            {
                return i;
            }
        }
        return 0;
    }

    [System.Serializable]
    public struct EngineSample
    {
        public int lowRev;
        public int highRev;
        public AudioClip clip;
        public AnimationCurve audioVolume;
        public AnimationCurve audioPitch;
        // Maybe find a way to replace that with a way to parametrize the formula
    }
    //because cant view a struct in a multidim. array on the editor you gotta make it into a class first
    [System.Serializable]
    public class EngineSampleArray
    {
        public EngineSample[] index;
    }

}


