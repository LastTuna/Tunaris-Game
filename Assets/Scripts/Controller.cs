using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
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
    public Canvas GoWashCanvas;

    public Canvas LoadingScreenCanvas;
    public static List<GameObject> currentCars = new List<GameObject>();
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
    public List<Material> carLogos;

    public void OpenGarage() {
        GoRaceCanvas.gameObject.SetActive(false);

        // Show the proper garage
        if (FindObjectOfType<DataController>().Garage3D) {
            // 3D garage
            Garage3DCanvas.gameObject.SetActive(true);

            // Can't be bothered
            string selectedCarName = FindObjectOfType<DataController>().SelectedCar;
            for (int i=0; i < carsPrefabs.Count; i++) {
                if (carsPrefabs[i].name == selectedCarName) {
                    CarIndex = i;
                    break;
                }
            }
            StartCoroutine(Move3DGarageModel(0));
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
    public GameObject Garage3DCarRoot;
    public GameObject Garage3DCarLogo;
    public GameObject Garage3DCarName;
    private int CarIndex = 0;
    public void Garage3DLeft() {
        CarIndex--;
        if (CarIndex < 0) CarIndex = carsPrefabs.Count - 1;
        StartCoroutine(Move3DGarageModel(2));
    }

    public void Garage3DRight() {
        CarIndex++;
        if (CarIndex > carsPrefabs.Count - 1) CarIndex = 0;
        StartCoroutine(Move3DGarageModel(-2));
    }

    public void TestInit() {
        // Can't be bothered
        string selectedCarName = FindObjectOfType<DataController>().SelectedCar;
        for (int i = 0; i < carsPrefabs.Count; i++) {
            if (carsPrefabs[i].name == selectedCarName) {
                CarIndex = i;
                break;
            }
        }
        StartCoroutine(Move3DGarageModel(0));
    }

    public IEnumerator Move3DGarageModel(float direction) {
        // accumulator to restore translation
        float acc = 0;
        if (direction != 0) {
            // Close door
            Garage3DModel.gameObject.GetComponent<GarageDoorScript>().CloseDoor();

            // Run "fake" translation
            while (Math.Abs(acc) < 70) {
                Garage3DModel.gameObject.transform.Translate(direction, 0, 0);
                acc += direction;
                yield return new WaitForEndOfFrame();
            }
        }

        // Set logo material
        Material logo = CarIndex > carLogos.Count - 2 ? carLogos[0] : carLogos[CarIndex+1];
        Garage3DCarLogo.GetComponent<MeshRenderer>().material = logo;

        // Set car name
        Garage3DCarName.GetComponent<Text>().text = carsPrefabs[CarIndex].name;

        // Delete old car model
        foreach(Transform child in Garage3DCarRoot.transform) {
            Destroy(child.gameObject);
        }

        // Instantiate new car model
        GameObject spinner = Instantiate(carsPrefabs[CarIndex], Garage3DCarRoot.transform);

        // Disable wheel colliders or unity spergs in the log
        foreach (WheelCollider wc in spinner.GetComponentsInChildren<WheelCollider>()) {
            Destroy(wc);
        }
        // Disable network scripts
        foreach (Behaviour c in spinner.GetComponents<NetworkTransform>()) {
            Destroy(c);
        }
        foreach (Behaviour c in spinner.GetComponents<NetworkTransformChild>()) {
            Destroy(c);
        }
        // Disable car driving scripts
        foreach (Behaviour c in spinner.GetComponents<Behaviour>()) {
            Destroy(c);
        }
        // Disable main rigidbody
        Destroy(spinner.GetComponent<Rigidbody>());

        // Set transform
        //spinner.transform.localScale = new Vector3(120, 120, 120);
        StartCoroutine(SetPosition(spinner));

        // Return to initial position and open door
        Garage3DModel.gameObject.transform.Translate(-acc, 0, 0);
        Garage3DModel.gameObject.GetComponent<GarageDoorScript>().OpenDoor();
    }

    private IEnumerator SetPosition(GameObject spinner) {
        // We do this thing because doing it all in Move3DGarageModel doesn't work
        // My guess is, all the scripts to be disabled (esp rigidbody) 
        // don't play nicely with setting the position
        yield return new WaitForEndOfFrame();
        spinner.transform.localPosition = new Vector3(0, 0, 0);
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
        GameObject.Find("Menu Audio Volume").GetComponent<Slider>().value = data.MenuAudio;
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
    //car wash save
    public void OpenWash()
    {
        GoWashCanvas.gameObject.SetActive(true);
        GoRaceCanvas.gameObject.SetActive(false);
        DataController dataController = FindObjectOfType<DataController>();
        int carIndex = GetIndex();
        string selectedCarName = dataController.SelectedCar;
        GameObject selectedCar = null;

        selectedCar = carsPrefabs[carIndex];
        selectedCar = Instantiate(carsPrefabs[carIndex], GoWashCanvas.transform);
        CarScriptKill(selectedCar);
        selectedCar.transform.localScale = new Vector3(100, 100, 100);
        selectedCar.transform.localPosition = new Vector3(0, -50, -200);
        // Add rotation script
        selectedCar.AddComponent<Spinner>();
        selectedCar.AddComponent<CarWash>();
        currentCars.Add(selectedCar);
    }
    public void WashMe()
    {
        DataController dataController = FindObjectOfType<DataController>();
        if (dataController.Cash >= 5)
        {
            StartCoroutine(Washer());
        }
    }
    public IEnumerator Washer()
    {
        DataController dataController = FindObjectOfType<DataController>();

        int carIndex = GetIndex();
        dataController.Cash += -5;
        FindObjectOfType<Spinner>().rotSpeed = 9f;
        while (dataController.Dirtiness[carIndex] > 0)
        {
        dataController.Dirtiness[carIndex] += -0.0005f;
        yield return new WaitForSeconds(0.1f);
        }
        if (dataController.Dirtiness[carIndex] < 0)
        {
        dataController.Dirtiness[carIndex] = 0;
        }
        FindObjectOfType<Spinner>().rotSpeed = 1f;
    }
    public int GetIndex()
    {
        int carIndex = 0;
        DataController dataController = FindObjectOfType<DataController>();
        foreach (GameObject e in carsPrefabs)
        {//get cars index
            if (dataController.SelectedCar.Equals(e.name))
            {
                break;
            }
            carIndex++;
        }
        return carIndex;
    }

    public void CarScriptKill (GameObject spinner)
    {
        // Disable wheel colliders or unity spergs in the log
        foreach (WheelCollider wc in spinner.GetComponentsInChildren<WheelCollider>())
        {
            Destroy(wc);
        }
        // Disable network scripts
        Destroy(spinner.GetComponent<NetworkTransform>());
        foreach (Behaviour c in spinner.GetComponents<NetworkTransformChild>())
        {
            Destroy(c);
        }
        // Disable car driving scripts
        foreach (Behaviour c in spinner.GetComponents<Behaviour>())
        {
            Destroy(c);
        }
        // Disable main rigidbody
        Destroy(spinner.GetComponent<Rigidbody>());
    }


    // Save options
    public void SaveOptions() {
        DataController data = GameObject.Find("DataController").GetComponent<DataController>();
        data.Garage3D = GameObject.Find("3D Garage").GetComponent<Toggle>().isOn;
        data.MenuAudio = GameObject.Find("Menu Audio Volume").GetComponent<Slider>().value;

        GameObject.Find("MenuAudio").GetComponent<MenuAudio>().SetVolume();

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

        //CarWash -> Go Race
        if (GoWashCanvas.gameObject.activeSelf)
        {
            foreach (GameObject carro in currentCars)
            {
                Destroy(carro);
            }
            GoWashCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);

        }

        yield return null;
    }
}
