using UnityEngine;

public class EngineAudioBehaviour : MonoBehaviour {
    // Car attached source
    public AudioSource CarEngine;

    // Engine sounds
    public EngineSample[] sounds;

    // Turbo related stuff
    public bool hasTurbo;
    public bool spooled;
    public float turboSpool = 0.1f;
    public AudioSource boostSource;
    public AudioClip woosh;
    public AudioClip pssh;

    // Some attempt at blindly speeding up processing
    private int lastIndex = 0;

    // Initialize audio
    void Start() {
        CarEngine.clip = sounds[lastIndex].clip;
        CarEngine.Play();

        
    }

    // Called from CarBehaviour, processes sounds
    public void ProcessSounds(float revs) {
        if (sounds.Length == 0) return;
        int currentIndex = lastIndex;

        // Which direction to go in the sound range
        if (sounds[currentIndex].lowRev > revs) {
            // Go down the sound range until revs fit (decelerating)
            while (sounds[currentIndex].lowRev > revs && lastIndex >= 0) {
                currentIndex--;
            }
        } else if (sounds[currentIndex].highRev < revs) {
            // Go down the sound range until revs fit (decelerating)
            while (sounds[currentIndex].highRev < revs && lastIndex < sounds.Length - 1) {
                currentIndex++;
            }
        }

        // At this point currentIndex points to the correct rev range
        // If it's a new sound, play it
        if (lastIndex != currentIndex) {
            CarEngine.clip = sounds[currentIndex].clip;
            CarEngine.pitch = 1f;
            CarEngine.Play();
            lastIndex = currentIndex;
        }

        // Apply the modifiers
        CarEngine.volume = sounds[currentIndex].relativeVolume;
        if (sounds[currentIndex].isPitchModified) {
            CarEngine.pitch = (revs / 1000) / 4;
        }

        // Process turbo sound
        if (hasTurbo) {
            // turbo started spooling
            if (revs > 830 && Input.GetAxis("Throttle") > 0)
            {//when you step on the gas
                boostSource.clip = woosh;
                boostSource.mute = false;
                boostSource.pitch = turboSpool;
                boostSource.Play();//PROBLEM - using the play() function has a delay, and the sound clips and creates a stuttering effect which doesnt sound nice. idea was to mute/unmute the soundbyte when it plays/doesnt play.
                //at its current state it has a delay, dunno how to fix it (calling boostSource.mute = true would also mute prematurely when playing wastegate.)
                if (turboSpool < 1.8f)//contol to keep boost level tops at 1.8
                {
                    turboSpool = turboSpool + 0.1f * (turboSpool / 2);
                }
                if (turboSpool > 1.3f)//after boost exceeds 1.3, play wastegate
                {
                    spooled = true;
                }
            }
            else
            {//when you let off the gas
                if (spooled)
                {
                    spooled = false;
                    boostSource.clip = pssh;
                    boostSource.pitch = 1;
                    boostSource.Play();
                    boostSource.loop = false;
                }
                turboSpool = 0.1f;

            }
        }

        }
    }

[System.Serializable]
public struct EngineSample {
    public int lowRev;
    public int highRev;
    public AudioClip clip;
    public float relativeVolume;
    // Maybe find a way to replace that with a way to parametrize the formula
    public bool isPitchModified;
}
