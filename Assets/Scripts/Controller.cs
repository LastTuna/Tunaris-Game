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
    public Canvas TuneScreenCanvas;

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
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameObject.Find("DataController").GetComponent<DataController>().SelectedCourse);
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
        GarageCanvas.gameObject.SetActive(true);

        string selectedCarName = GameObject.Find("DataController").GetComponent<DataController>().SelectedCar;

        // Add the car buttons
        createdGarageButtons = new List<GameObject>();
        int offset = 0;
        GameObject selectedCar = null;
        foreach(GameObject prefab in carsPrefabs) {
            GameObject button = Instantiate(buttonPrefab, GarageCanvas.transform);

            // Set labels
            button.name = prefab.name;
            button.GetComponentInChildren<Text>().text = prefab.name;

            // Set car model instantiation parameters
            button.GetComponent<ButtonProperties>().carPrefab = prefab;
            button.GetComponent<ButtonProperties>().parent = GarageCanvas;
            createdGarageButtons.Add(button);

            // Move the bottm to its correct position using a lot of unity code soup
            (button.transform as RectTransform).anchoredPosition = new Vector2((button.transform as RectTransform).anchoredPosition.x, (button.transform as RectTransform).anchoredPosition.y + offset);
            offset -= 70;
        }

        // Set focus to the button corresponding to the last selected car
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(selectedCar == null ? createdGarageButtons[0] : selectedCar);

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

    // Main menu button callbacks
    public void GoRace() {
        MainMenuCanvas.gameObject.SetActive(false);
        GoRaceCanvas.gameObject.SetActive(true);
    }

    public void Options() {
        MainMenuCanvas.gameObject.SetActive(false);
        OptionsCanvas.gameObject.SetActive(true);
    }

    public void ExitGame() {
        Application.Quit();
    }

    // Course select callback
    public void CourseSelection() {
        Debug.Log(gameObject.GetComponent<EventSystem>().currentSelectedGameObject);
        GameObject.Find("DataController").GetComponent<DataController>().SelectedCourse = gameObject.GetComponent<EventSystem>().currentSelectedGameObject.name;
        Cancel();
    }

    // Car selection callback
    public void CarSelection() {
        Debug.Log(gameObject.GetComponent<EventSystem>().lastSelectedGameObject);
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

    // Global cancel callback
    public void Cancel() {
        // Don't return past main menu
        if (MainMenuCanvas.gameObject.activeSelf) return;

        // Always save settings
        GameObject.Find("DataController").GetComponent<DataController>().SaveGameData();

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
        }

        // Tuning -> Go Race
        if (TuneScreenCanvas.gameObject.activeSelf) {
            TuneScreenCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);
        }
    }
}
