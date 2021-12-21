using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MobileButtonHandler : MonoBehaviour {
    public TouchScreenButton[] ScreenButtons;

	void Start () {
        foreach(TouchScreenButton dolor in ScreenButtons)
        {
            //get user button scaling and pos from user settings (datacontroller?)

        }


	}
	

	void FixedUpdate () {
        //ya insain
        TouchHandler();
	}

    public void TouchHandler()
    {
        int activeTouches = Input.touchCount;
        //get every touch pos and check if its touching a button
        for(int i  = 0; i < activeTouches; i++)
        {
            if(Input.GetTouch(i).phase == TouchPhase.Began)
            {
                ButtonChecker(Input.GetTouch(i).position);
                //iterate through every button and collision check
                //do i NEED to check the touch phase?
                //Input.GetTouch(i).position;
            }
        }
    }
    
    void ButtonChecker(Vector2 fingerPos)
    {
        foreach (TouchScreenButton dolor in ScreenButtons)
        {
            Debug.Log(dolor.button.GetComponent<Image>().sprite.bounds.ToString());
            if(dolor.button.GetComponent<Image>().sprite.bounds.Contains(new Vector3(fingerPos.x, fingerPos.y, 0))){
                //write here button payload
                CustomInput.SetAxis(dolor.axis , 1);
            }


        }

    }

}

[System.Serializable]
public class TouchScreenButton
{
    public GameObject button;
    public string axis;//the name of the input axis we is overriding (ex. "Throttle", "Brake", "Clutch")
    //fetched with GetComponent<image>().size during runtime or whatever
    public float buttonValue = 1;
    public Vector2 userButtonPos;//(user) position on screen.
    public Vector2 userButtonScale;//user scale

}
