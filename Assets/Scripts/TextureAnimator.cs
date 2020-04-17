using UnityEngine;

public class TextureAnimator : MonoBehaviour {
    
    public int framerate = 3;//every x update animation gets refreshed
    public int currentFrame = 0;//current frame
    public int framecount = 16;//how many frames does the texture have? this to prevent overflow
    public int counter = 0;
	
	// Update is called once per frame
	void FixedUpdate () {
        if(counter > framerate)
        {
            counter = 0;
            currentFrame++;
            if (currentFrame > framecount - 1) currentFrame = 0;//reset to origin position to prevent possible overflow
            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(currentFrame * (1 / framecount), 0);
        }
        counter++;
    }
}
