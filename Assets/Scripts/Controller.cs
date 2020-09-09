using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
    public SetupManager TuneManager;
    public Canvas MainMenuCanvas;
    public Canvas GoRaceCanvas;
    public Canvas OptionsCanvas;

    public Canvas CuntUI;

    public Canvas CourseSelectCanvas;
    public Canvas GarageCanvas;
    public GameObject Garage3DCanvas;
    public Canvas TuneScreenCanvas;
    public Canvas LoadTuneScreenCanvas;
    public Canvas SaveTuneScreenCanvas;
    public Canvas OverwriteTuneScreenCanvas;

    public Canvas OnlineCanvas;
    public Canvas GoWashCanvas;
    public Canvas CreditsCanvas;
    public Canvas RecordsCanvas;
    public Canvas LicensesCanvas;
    public GameObject setupExceptionText;
    public static bool washing;
    public Canvas LoadingScreenCanvas;
    public AudioSource menuMusic;//controlling menu music temporaily via controller. make music manager later on
    public AudioClip[] TGmusic;
    public static List<GameObject> currentCars = new List<GameObject>();
    public GameObject carro;
    public GameObject bestest;
    
    
    public void DefaultCallback() {
        Debug.Log("you forgot to set a click callback you retard");
    }

    void Update()
    {
        TGControlTest();
    }

    public void TGControlTest()
    {
        DataController data = FindObjectOfType<DataController>();
        if (MainMenuCanvas.gameObject.activeSelf)
        {
            if (Input.GetKeyDown("t") && !bestest.activeSelf)
            {
                KeyValuePair<string, Laptime> fastestLap = new KeyValuePair<string, Laptime>("Star GT V8", TimeSpan.FromSeconds(1488));
                foreach (var carpair in data.BestestLapTimes)
                {
                    fastestLap = carpair;
                }
                carro = Instantiate(carsPrefabs.Find(carpre => carpre.name == fastestLap.Key), new Vector3(0, -1.4f, -2.4f), Quaternion.Euler(0, -90, 15));
                CarScriptKill(carro);
                bestest.SetActive(true);
                bestest.GetComponent<TextMesh>().text = string.Format("{0:00}:{1:00}:{2:000}", fastestLap.Value.Minutes, fastestLap.Value.Seconds, fastestLap.Value.Milliseconds);
            }
            else if (Input.GetKeyDown("t") && bestest.activeSelf)
            {
                Destroy(carro);
                bestest.SetActive(false);
            }
        }
        if (!MainMenuCanvas.gameObject.activeSelf && bestest.activeSelf)
        {
            bestest.SetActive(false);
            Destroy(carro);
        }
    }

    // Go race callbacks
    public void StartRace() {
        LoadingScreenCanvas.gameObject.SetActive(true);
        GoRaceCanvas.gameObject.SetActive(false);
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
        if (FindObjectOfType<DataController>().CuntUI) {
            CuntUI.gameObject.SetActive(false);
        } else {
            GoRaceCanvas.gameObject.SetActive(false);
        }

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
        CarScriptKill(spinner);

        // Set transform
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
        DataController data = FindObjectOfType<DataController>();
        TuneManager.LoadSetupUI(data.SelectedCar);//load car specific setup ui
        
    }

    public void OpenLoadTuneScreen()
    {
        TuneManager.UnLoadSetupUI();
        TuneManager.LoadSetupsList();
        TuneScreenCanvas.gameObject.SetActive(false);
        LoadTuneScreenCanvas.gameObject.SetActive(true);
        //here add a method call from setup manager to print all available setups.
    }

    public void OpenSaveTuneScreen()
    {
        TuneManager.UnLoadSetupUI();
        TuneScreenCanvas.gameObject.SetActive(false);
        SaveTuneScreenCanvas.gameObject.SetActive(true);
        //here add a method call from setup manager to print all available setups.
    }

    public void SaveTune(bool overwrite)
    {
        //when tune save screens save button is pressed - false
        //when tune overwrite button is pressed - true
        string setupNameField = GameObject.Find("varSetupName").GetComponent<Text>().text;
        string setupDesc = GameObject.Find("varSetupDesc").GetComponent<Text>().text;
        //make sure there is a setup name
        if (setupNameField != "")
        {
            //check if setup with that name exists, if not proceed to save setup
            if (!TuneManager.SetupDuplicateChecker(setupNameField) || overwrite)
            {
                TuneManager.SaveSetup(setupNameField, setupDesc);
                //"setup saved successfully"
                StartCoroutine(TuneException("Setup " + setupNameField + " saved successfully."));
                OverwriteTuneScreenCanvas.gameObject.SetActive(false);
                Cancel();
            }
            else
            {
                //open "do you want to overwrite this tune"
                OverwriteTuneScreenCanvas.gameObject.SetActive(!overwrite);
            }
        }
        else
        {
        StartCoroutine(TuneException("Setup needs a name."));
            //"setup needs a name"
        }
    }

    public void CancelSetupOverwrite()
    {
        SaveTuneScreenCanvas.gameObject.SetActive(true);
        OverwriteTuneScreenCanvas.gameObject.SetActive(false);
    }

    //setup needs a name text
    public IEnumerator TuneException(string text)
    {
        GameObject dolor = Instantiate(setupExceptionText);
        dolor.GetComponentInChildren<Text>().text = text;
        yield return new WaitForSeconds(2);
        Destroy(dolor);
    }
    
    public void OpenOnlineScreen()
    {
        GoRaceCanvas.gameObject.SetActive(false);
        OnlineCanvas.gameObject.SetActive(true);
        //fetch values from save data
        DataController data = FindObjectOfType<DataController>();
        GameObject.Find("IP Input").GetComponent<InputField>().text = data.IP;
        GameObject.Find("Username Input").GetComponent<InputField>().text = data.PlayerName;

    }


    // Main menu button callbacks
    public void GoRace() {
        StartCoroutine(GoRaceCoroutine());
    }

    IEnumerator GoRaceCoroutine() {
        // Waiting for the end of the frame ensures the Pressed state of the FSM is entered, and the select sound being played
        yield return new WaitForEndOfFrame();
        MainMenuCanvas.gameObject.SetActive(false);
        DataController dataController = FindObjectOfType<DataController>();
        if (dataController.CuntUI) {
            CuntUI.gameObject.SetActive(true);
        } else {
            GoRaceCanvas.gameObject.SetActive(true);
        }
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
        DataController data = FindObjectOfType<DataController>();
        GameObject.Find("3D Garage").GetComponent<Toggle>().isOn = data.Garage3D;
        GameObject.Find("Menu Audio Volume").GetComponent<Slider>().value = data.MenuAudio;
    }

    public void Licenses() {
        StartCoroutine(LicensesCoroutine());
    }

    IEnumerator LicensesCoroutine() {
        // Waiting for the end of the frame ensures the Pressed state of the FSM is entered, and the select sound being played
        yield return new WaitForEndOfFrame(); 
        
        GoRaceCanvas.gameObject.SetActive(false);
        LicensesCanvas.gameObject.SetActive(true);

        //fetch values from save data
        DataController data = FindObjectOfType<DataController>();
        GameObject.Find("busrider").GetComponent<Toggle>().isOn = data.IsBusrider;
    }

    public void Records() {
        StartCoroutine(RecordsCoroutine());
    }
    IEnumerator RecordsCoroutine() {
        // Waiting for the end of the frame ensures the Pressed state of the FSM is entered, and the select sound being played
        yield return new WaitForEndOfFrame();
        MainMenuCanvas.gameObject.SetActive(false);
        RecordsCanvas.gameObject.SetActive(true);

        //fetch values from save data
        DataController data = FindObjectOfType<DataController>();
        KeyValuePair<string, Laptime> fastestLap = new KeyValuePair<string, Laptime>("Nasan GRT", TimeSpan.FromSeconds(10));
        foreach (var carpair in data.BestestLapTimes) {
            fastestLap = carpair;
            break;
        }
        if (carro != null) Destroy(carro);
        carro = Instantiate(carsPrefabs.Find(carpre => carpre.name == fastestLap.Key), Vector3.zero, Quaternion.Euler(0, -90, 15), RecordsCanvas.transform);
        carro.transform.localScale = new Vector3(3, 3, 3);
        carro.transform.localPosition = new Vector3(-0.76f, -0.6f, -922.1f);
        CarScriptKill(carro);
        bestest.GetComponent<TextMesh>().text = string.Format("{0:00}:{1:00}:{2:000}", fastestLap.Value.Minutes, fastestLap.Value.Seconds, fastestLap.Value.Milliseconds);
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
        FindObjectOfType<DataController>().SelectedCourse = gameObject.GetComponent<EventSystem>().currentSelectedGameObject.name;
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
        FindObjectOfType<DataController>().SelectedCar = gameObject.GetComponent<EventSystem>().currentSelectedGameObject.name;
        Cancel();
    }
    
    public void OpenCredits()
    {
        GoRaceCanvas.gameObject.SetActive(false);
        CreditsCanvas.gameObject.SetActive(true);
        menuMusic.clip = TGmusic[1];
        menuMusic.Play();
    }

    //online IP validation. this is called from Cancel().
    public void ValidateOnline()
    {
        DataController data = FindObjectOfType<DataController>();
        data.IP = GameObject.Find("IP Address").GetComponent<Text>().text;
        data.PlayerName = GameObject.Find("Username").GetComponent<Text>().text;
        data.SaveGameData();
    }
    //car wash save
    public void OpenWash()
    {
        GoWashCanvas.gameObject.SetActive(true);
        GoRaceCanvas.gameObject.SetActive(false);
        menuMusic.clip = TGmusic[2];
        menuMusic.Play();
        DataController dataController = FindObjectOfType<DataController>();
        GameObject selectedCar = Instantiate(carsPrefabs.Find(carpre => carpre.name == dataController.SelectedCar), GoWashCanvas.transform);
        CarScriptKill(selectedCar);
        selectedCar.transform.localScale = new Vector3(90, 90, 90);
        selectedCar.transform.localPosition = new Vector3(0, -150, -326);
        // Add rotation script
        selectedCar.AddComponent<Spinner>();
        selectedCar.AddComponent<CarWash>();
        currentCars.Add(selectedCar);
    }
    public void WashMe()
    {
        DataController dataController = FindObjectOfType<DataController>();
        if (dataController.Cash >= 5 && dataController.GetDirtiness() > 0.03f && !washing)
        {
            washing = true;
            StartCoroutine(Washer());
        }
    }
    public IEnumerator Washer()
    {
        DataController dataController = FindObjectOfType<DataController>();

        dataController.Cash += -5;
        FindObjectOfType<Spinner>().rotSpeed = 9f;
        while (dataController.GetDirtiness() > 0)
        {
        dataController.AddDirtiness(-0.02f);
        yield return new WaitForSeconds(0.1f);
        }
        if (dataController.GetDirtiness() < 0)
        {
        dataController.SetDirtiness(0);
        }
        FindObjectOfType<Spinner>().rotSpeed = 1f;
        washing = false;
    }

    public void CarScriptKill (GameObject spinner)
    {
        // Disable wheel colliders or unity spergs in the log
        foreach (WheelCollider wc in spinner.GetComponentsInChildren<WheelCollider>())
        {
            Destroy(wc);
        }
        // Disable car driving scripts
        foreach (Behaviour c in spinner.GetComponents<Behaviour>())
        {
            Destroy(c);
        }
        // Disable network scripts
        foreach (Behaviour c in spinner.GetComponents<NetworkTransformChild>())
        {
            Destroy(c);
        }
        Destroy(spinner.GetComponent<NetworkTransform>());
        Destroy(spinner.GetComponent<NetworkIdentity>());
        Debug.Log("ignore error, only occurs due to script kill method removing network behavior in wrong order");
        //disable tyre behavior
        foreach (Behaviour c in spinner.GetComponentsInChildren<TireBehavior>())
        {
            Destroy(c);
        }
        // Disable main rigidbody
        Destroy(spinner.GetComponent<Rigidbody>());
    }    

    // Save options
    public void SaveOptions() {
        DataController data = FindObjectOfType<DataController>();
        data.Garage3D = GameObject.Find("3D Garage").GetComponent<Toggle>().isOn;
        data.MenuAudio = GameObject.Find("Menu Audio Volume").GetComponent<Slider>().value;
        data.CuntUI = GameObject.Find("ImAFuckingCunt").GetComponent<Toggle>().isOn;

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
        DataController dataController = FindObjectOfType<DataController>();

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
            if (dataController.CuntUI) {
                CuntUI.gameObject.SetActive(true);
            } else {
                GoRaceCanvas.gameObject.SetActive(true);
            }
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
            TuneManager.UnLoadSetupUI();
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
            menuMusic.clip = TGmusic[0];
            menuMusic.Play();
        }
        //Credits -> Go Race
        if(CreditsCanvas.gameObject.activeSelf)
        {
            CreditsCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);
            menuMusic.clip = TGmusic[0];
            menuMusic.Play();
        }

        // Licenses -> Go Race
        if (LicensesCanvas.gameObject.activeSelf) {
            dataController.LoadedData.IsBusrider = GameObject.Find("busrider").GetComponent<Toggle>().isOn;

            LicensesCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);

            menuMusic.clip = TGmusic[0];
            menuMusic.Play();
        }

        // Records -> Main
        if (RecordsCanvas.gameObject.activeSelf) {
            RecordsCanvas.gameObject.SetActive(false);
            MainMenuCanvas.gameObject.SetActive(true);
        }

        // Setup -> Go Race
        if (TuneScreenCanvas.gameObject.activeSelf)
        {
            TuneScreenCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);
        }

        // Load Setup -> Setup
        if (LoadTuneScreenCanvas.gameObject.activeSelf)
        {
            LoadTuneScreenCanvas.gameObject.SetActive(false);
            OpenTuneScreen();
        }

        // Save Setup -> Setup
        if (SaveTuneScreenCanvas.gameObject.activeSelf)
        {
            SaveTuneScreenCanvas.gameObject.SetActive(false);
            OpenTuneScreen();
        }

        // Overwrite Setup -> Setup
        if (OverwriteTuneScreenCanvas.gameObject.activeSelf)
        {
            SaveTuneScreenCanvas.gameObject.SetActive(false);
            OpenTuneScreen();
        }


        dataController.SaveGameData();
        yield return null;
    }
}
