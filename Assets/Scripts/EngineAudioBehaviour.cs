using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudioBehaviour : MonoBehaviour
{
    // Car attached source
    public AudioSource[] CarEngine;
    public AudioSource boostSource;
    public AudioSource hornSource;
    //can essentially have as many sound layers as you want
    
    // Engine sounds
    public EngineSoundData AccelSounds;
    public EngineSoundData DecelSounds;
    //MULTIDIMENSIONAL ARRAY
    //FIRST COMES INDEX
    //SECOND COMES LAYER
    public int[] rpmIndex;
    //index the sounds by rpm range for quickly finding the relevant sound samples

    // Turbo related stuff
    public bool hasTurbo;
    public bool isSpooled;

    // Some attempt at blindly speeding up processing
    private int lastIndex = 0;
    
    //called from carbehavior. gets a shitload of required resources etc..
    public void InitiateSounds(AssetBundle car)
    {
        AccelSounds = new EngineSoundData();
        AccelSounds = AccelSounds.ImportData(null);
        LoadSamples(AccelSounds,car);

        AccelSounds.boost = car.LoadAsset<AudioClip>(AccelSounds.boostName);
        AccelSounds.horn = car.LoadAsset<AudioClip>(AccelSounds.hornName);
        //load psshh sounds
        AudioClip[] tempPshh = new AudioClip[AccelSounds.pshhNames.Length];//jut make a temp array for them
        for(int i = 0; i < AccelSounds.pshhNames.Length; i++)
        {
            tempPshh[i] = car.LoadAsset<AudioClip>(AccelSounds.pshhNames[i]);
        }
        //initiate rpm ref index
        rpmIndex = new int[AccelSounds.SampleIndex.Length];
        for (int i = 0; i < AccelSounds.SampleIndex.Length; i++)
        {
            rpmIndex[i] = AccelSounds.SampleIndex[i].indexThreshold;
        }
    }

    //htis iterates through every sample on every layer/index n gets the resources n applies to its place.
    public void LoadSamples(EngineSoundData soundbank, AssetBundle car)
    {
        foreach(EngineSampleIndex penoz in soundbank.SampleIndex)
        {
            foreach(EngineSample layer in penoz.SampleLayer)
            {
                if (layer.clipname == "") break;
                layer.clip = car.LoadAsset<AudioClip>(layer.clipname);
            }
        }
    }
    
    // Called from CarBehaviour, processes sounds
    public void ProcessSounds(float revs, bool spooled, float boostPressure, float throttlePressure)
    {
        if (AccelSounds.SampleIndex.Length == 0) return;//null check

        //find index
        int currentIndex = SoundIndex(revs);
        // At this point currentIndex points to the correct rev range

        // If it's a new sound, play it
        if (lastIndex != currentIndex)
        {
            //go through all layers in current index and apply samples. mute unused layers
            int i = 0;
            for (i = 0; i < AccelSounds.SampleIndex[currentIndex].SampleLayer.Length; i++)
            {
                CarEngine[i].Play();
            }
            while (i < CarEngine.Length)
            {
                CarEngine[i].clip = null;//mute unused
                i++;
            }
            lastIndex = currentIndex;
        }
        // Apply the modifiers
        for (int i = 0; i < AccelSounds.SampleIndex[currentIndex].SampleLayer.Length; i++)
        {
            float lerpFactor = Mathf.Lerp(AccelSounds.SampleIndex[currentIndex].SampleLayer[i].lowRPM, AccelSounds.SampleIndex[currentIndex].SampleLayer[i].hiRPM, revs);
            //lerp factor,used in pitching and volume adjustment
            CarEngine[i].pitch = AccelSounds.SampleIndex[currentIndex].SampleLayer[i].audioPitch.Evaluate(lerpFactor);
            CarEngine[i].volume = AccelSounds.SampleIndex[currentIndex].SampleLayer[i].audioVolume.Evaluate(lerpFactor);
        }
        // Process turbo sound
        if (hasTurbo)
        {
            // turbo whine (your mom)
            if (!isSpooled && spooled)
            {
                boostSource.clip = AccelSounds.boost;
                boostSource.pitch = boostPressure;
                boostSource.loop = true;
                boostSource.Play();
                isSpooled = spooled;
            }

            // turbo wastegate PSSSHHHHHHH once lift gas AND boost threshold has been crossed
            if (isSpooled && !spooled)
            {
                boostSource.pitch = 1;
                boostSource.clip = AccelSounds.pshh[UnityEngine.Random.Range(0, AccelSounds.pshh.Length)];
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
}