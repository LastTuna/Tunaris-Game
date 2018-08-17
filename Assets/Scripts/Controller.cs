﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
    public Canvas MainMenuCanvas;
    public Canvas GoRaceCanvas;
    public Canvas OptionsCanvas;

    public Canvas CourseSelectCanvas;
    public Canvas GarageCanvas;
    public GameObject Garage3DCanvas;
    public Canvas TuneScreenCanvas;
    public Canvas OnlineCanvas;

    public Canvas LoadingScreenCanvas;


    public void DefaultCallback() {
        Debug.Log("you forgot to set a click callback you retard");
    }

    // Go race callbacks
    public void StartRace() {
        LoadingScreenCanvas.gameObject.SetActive(true);
        StartCoroutine(LoadRace());
    }

    IEnumerator LoadRace() {
        yield return new WaitForSeconds(5);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(FindObjectOfType<DataController>().SelectedCourse);
        while (!asyncLoad.isDone) {
            yield return null;
        }
    }

    public void CourseSelect() {
        GoRaceCanvas.gameObject.SetActive(false);
        CourseSelectCanvas.gameObject.SetActive(true);
    }

    private List<GameObject> createdGarageButtons;
    public GameObject buttonPrefab;
    public List<GameObject> carsPrefabs;

    public void OpenGarage() {
        GoRaceCanvas.gameObject.SetActive(false);

        // Show the proper garage
        if (FindObjectOfType<DataController>().Garage3D) {
            // 3D garage
            Garage3DCanvas.gameObject.SetActive(true);

            for(int i=0; i < carsPrefabs.Count; i++) {

            }
        } else {
            // Classic GT2 garage
            GarageCanvas.gameObject.SetActive(true);

            string selectedCarName = FindObjectOfType<DataController>().SelectedCar;

            // Add the car buttons
            createdGarageButtons = new List<GameObject>();
            int offset = 0;
            GameObject selectedCar = null;
            foreach (GameObject prefab in carsPrefabs) {
                GameObject button = Instantiate(buttonPrefab, GarageCanvas.transform);

                // Set labels
                button.name = prefab.name;
                button.GetComponentInChildren<Text>().text = prefab.name;

                // Set car model instantiation parameters
                button.GetComponent<ButtonProperties>().carPrefab = prefab;
                button.GetComponent<ButtonProperties>().parent = GarageCanvas;
                createdGarageButtons.Add(button);

                // Add car select callback
                button.GetComponent<Button>().onClick.AddListener(CarSelection);

                // Move the button to its correct position using a lot of unity code soup
                (button.transform as RectTransform).anchoredPosition = new Vector2((button.transform as RectTransform).anchoredPosition.x, (button.transform as RectTransform).anchoredPosition.y + offset);
                offset -= 70;

                // Detect default focused button
                if (prefab.name == selectedCarName) selectedCar = button;
            }

            // Set focus to the button corresponding to the last selected car
            StartCoroutine(SetSelectedGameObject(selectedCar));
        }
    }

    public GameObject Garage3DModel;
    private int CarIndex = 0;
    public void Garage3DLeft() {
        StartCoroutine(Move3DGarageModel(2));
    }

    public void Garage3DRight() {
        StartCoroutine(Move3DGarageModel(-2));
    }

    public IEnumerator Move3DGarageModel(float direction) {
        Garage3DModel.gameObject.GetComponent<GarageDoorScript>().CloseDoor();
        float acc = 0;
        while(Math.Abs(acc) < 70) {
            Garage3DModel.gameObject.transform.Translate(direction, 0, 0);
            acc += direction;
            yield return new WaitForEndOfFrame();
        }
        Garage3DModel.gameObject.transform.Translate(-acc, 0, 0);
        Garage3DModel.gameObject.GetComponent<GarageDoorScript>().OpenDoor();
    }

    private IEnumerator SetSelectedGameObject(GameObject select) {
        yield return new WaitForEndOfFrame();
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(select);
    }
    
    public void OpenTuneScreen() {
        GoRaceCanvas.gameObject.SetActive(false);
        TuneScreenCanvas.gameObject.SetActive(true);

        // Restore tuning values from save data
        DataController data = GameObject.Find("DataController").GetComponent<DataController>();
        GameObject.Find("Tire Bias").GetComponent<Slider>().value = data.TireBias;
        GameObject.Find("Final Drive").GetComponent<Slider>().value = data.FinalDrive;
        GameObject.Find("Aero").GetComponent<Slider>().value = data.Aero;
        GameObject.Find("Spring Stiffness").GetComponent<Slider>().value = data.SpringStiffness;
        GameObject.Find("Brake Stiffness").GetComponent<Slider>().value = data.BrakeStiffness;
        GameObject.Find("Gearbox Selector").GetComponent<Dropdown>().value = (int)data.Gearbox;
    }

    public void OpenOnlineScreen()
    {
        GoRaceCanvas.gameObject.SetActive(false);
        OnlineCanvas.gameObject.SetActive(true);
        //fetch values from save data
        DataController data = GameObject.Find("DataController").GetComponent<DataController>();
        GameObject.Find("IP Address").GetComponent<Text>().text = data.IP;
        GameObject.Find("Username").GetComponent<Text>().text = data.PlayerName;

    }


    // Main menu button callbacks
    public void GoRace() {
        StartCoroutine(GoRaceCoroutine());
    }

    IEnumerator GoRaceCoroutine() {
        // Waiting for the end of the frame ensures the Pressed state of the FSM is entered, and the select sound being played
        yield return new WaitForEndOfFrame();
        MainMenuCanvas.gameObject.SetActive(false);
        GoRaceCanvas.gameObject.SetActive(true);
    }

    public void Options() {
        StartCoroutine(OptionsCoroutine());
    }
    IEnumerator OptionsCoroutine() {
        // Waiting for the end of the frame ensures the Pressed state of the FSM is entered, and the select sound being played
        yield return new WaitForEndOfFrame();
        MainMenuCanvas.gameObject.SetActive(false);
        OptionsCanvas.gameObject.SetActive(true);

        //fetch values from save data
        DataController data = GameObject.Find("DataController").GetComponent<DataController>();
        GameObject.Find("3D Garage").GetComponent<Toggle>().isOn = data.Garage3D;
    }

    public void ExitGame() {
        Application.Quit();
    }

    // Course select callback
    public void CourseSelection() {
        StartCoroutine(CourseSelectionCoroutine());
    }

    IEnumerator CourseSelectionCoroutine() {
        // Waiting for the end of the frame ensures the Pressed state of the FSM is entered, and the select sound being played
        yield return new WaitForEndOfFrame();

        Debug.Log(gameObject.GetComponent<EventSystem>().currentSelectedGameObject);
        GameObject.Find("DataController").GetComponent<DataController>().SelectedCourse = gameObject.GetComponent<EventSystem>().currentSelectedGameObject.name;
        Cancel();
    }

    // Car selection callback
    public void CarSelection() {
        StartCoroutine(CarSelectionCoroutine());
    }

    IEnumerator CarSelectionCoroutine() {
        // Waiting for the end of the frame ensures the Pressed state of the FSM is entered, and the select sound being played
        yield return new WaitForEndOfFrame();
        Debug.Log(gameObject.GetComponent<EventSystem>().currentSelectedGameObject);
        GameObject.Find("DataController").GetComponent<DataController>().SelectedCar = gameObject.GetComponent<EventSystem>().currentSelectedGameObject.name;
        Cancel();
    }

    // Tuning validation
    public void ValidateTuning() {
        DataController data = GameObject.Find("DataController").GetComponent<DataController>();
        data.TireBias = GameObject.Find("Tire Bias").GetComponent<Slider>().value;
        data.FinalDrive = GameObject.Find("Final Drive").GetComponent<Slider>().value;
        data.Aero = GameObject.Find("Aero").GetComponent<Slider>().value;
        data.SpringStiffness = GameObject.Find("Spring Stiffness").GetComponent<Slider>().value;
        data.BrakeStiffness = GameObject.Find("Brake Stiffness").GetComponent<Slider>().value;
        data.Gearbox = GameObject.Find("Gearbox Selector").GetComponent<Dropdown>().value;
        
        Cancel();
    }
    //online IP validation. this is called from Cancel().
    public void ValidateOnline()
    {
        DataController data = GameObject.Find("DataController").GetComponent<DataController>();
        data.IP = GameObject.Find("IP Address").GetComponent<Text>().text;
        data.PlayerName = GameObject.Find("Username").GetComponent<Text>().text;
    }

    // Save options
    public void SaveOptions() {
        DataController data = GameObject.Find("DataController").GetComponent<DataController>();
        data.Garage3D = GameObject.Find("3D Garage").GetComponent<Toggle>().isOn;
        Debug.Log(data.Garage3D);
        Cancel();
    }

    // Global cancel callback
    public AudioClip cancelClip;
    public void Cancel() {
        StartCoroutine(CancelCoroutine());
    }
    public IEnumerator CancelCoroutine() {
        // Don't return past main menu
        if (MainMenuCanvas.gameObject.activeSelf) yield return null;

        // Always save settings
        GameObject.Find("DataController").GetComponent<DataController>().SaveGameData();

        // Play cancel sound
        AudioSource audioSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        audioSource.PlayOneShot(cancelClip);

        // Wait for the sound to start playing, just in case
        yield return new WaitForEndOfFrame();

        // Go Race -> menu
        if (GoRaceCanvas.gameObject.activeSelf) {
            MainMenuCanvas.gameObject.SetActive(true);
            GoRaceCanvas.gameObject.SetActive(false);
        }

        // Options -> menu
        if (OptionsCanvas.gameObject.activeSelf) {
            MainMenuCanvas.gameObject.SetActive(true);
            OptionsCanvas.gameObject.SetActive(false);
        }

        // Course -> Go Race
        if (CourseSelectCanvas.gameObject.activeSelf) {
            GoRaceCanvas.gameObject.SetActive(true);
            CourseSelectCanvas.gameObject.SetActive(false);
        }

        // Garage -> Go Race
        if (GarageCanvas.gameObject.activeSelf) {
            GoRaceCanvas.gameObject.SetActive(true);
            GarageCanvas.gameObject.SetActive(false);

            foreach(GameObject button in createdGarageButtons) {
                Destroy(button);
            }
            createdGarageButtons.Clear();
        }

        // Garage 3D -> Go Race
        if (Garage3DCanvas.gameObject.activeSelf) {
            GoRaceCanvas.gameObject.SetActive(true);
            Garage3DCanvas.gameObject.SetActive(false);
        }

        // Tuning -> Go Race
        if (TuneScreenCanvas.gameObject.activeSelf) {
            TuneScreenCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);
        }

        // Online -> Go Race
        if (OnlineCanvas.gameObject.activeSelf)
        {
            ValidateOnline();
            OnlineCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);
        }

        yield return null;
    }
}
