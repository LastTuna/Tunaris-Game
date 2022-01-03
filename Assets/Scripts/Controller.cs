using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public GameObject GarageTurntableParent;
    public Canvas GarageTurntableCanvas;
    public Canvas GarageContextCanvas;
    public Canvas GarageOKCanvas;
    public Canvas TuneScreenCanvas;
    public Canvas LoadTuneScreenCanvas;
    public Canvas SaveTuneScreenCanvas;
    public Canvas OverwriteTuneScreenCanvas;

    public Canvas OnlineCanvas;
    public Canvas GoWashCanvas;
    public Canvas CreditsCanvas;
    public Canvas RecordsCanvas;
    public Canvas LicensesCanvas;
    public GameObject GarageScrollBoxContent;
    public GameObject setupExceptionText;
    public static bool washing;
    public Canvas LoadingScreenCanvas;
    public AudioSource menuMusic;//controlling menu music temporaily via controller. make music manager later on
    public AudioClip[] TGmusic;
    public List<GameObject> currentCars = new List<GameObject>();//just a container for any instantiated cars so they can be cleaned up.
    public GameObject carro;
    public GameObject bestest;
    public CarData GarageCarInfo;
    string garageCarPrefabName;//container for the prefab name of the car u just picked
    
    public void DefaultCallback() {
        Debug.Log("you forgot to set a click callback you retard");
    }

    void Update() {
        TGControlTest();
    }

    public void TGControlTest()
    {
        DataController data = FindObjectOfType<DataController>();
        if (MainMenuCanvas.gameObject.activeSelf)
        {
            if (Input.GetKeyDown("t") && !bestest.activeSelf)
            {
                GameObject pos = new GameObject();
                pos.transform.SetPositionAndRotation(new Vector3(0, -1.4f, -2.4f), Quaternion.Euler(0, -90, 15));
                carro = InstantiateCar("rs200", pos.transform);
                bestest.SetActive(true);
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
        StartCoroutine(OpenGarageImpl());
    }

    public IEnumerator OpenGarageImpl() {
        yield return new WaitForEndOfFrame();
        if (FindObjectOfType<DataController>().CuntUI) {
            CuntUI.gameObject.SetActive(false);
        } else {
            GoRaceCanvas.gameObject.SetActive(false);
        }
        menuMusic.clip = TGmusic[3];
        menuMusic.Play();

        // Classic GT2 garage
        ContentManager cm = FindObjectOfType<ContentManager>();
        cm.UnloadAllContent();
        string[] installedCars = cm.LoadManifest();

        GarageCanvas.gameObject.SetActive(true);
        // Add the car buttons
        createdGarageButtons = new List<GameObject>();
        int offset = 0;

        //go through every car found in the manifest and create the buttons for them
        foreach (string prefab in installedCars) {
            GameObject button = Instantiate(buttonPrefab, GarageScrollBoxContent.transform);
            // Set labels
            button.name = prefab;
            button.GetComponentInChildren<Text>().text = cm.GetCarName(prefab, true);

            //write a script on the button to duz its thing

            createdGarageButtons.Add(button);
            // Add car select callback
            button.GetComponent<Button>().onClick.AddListener(CarSelection);

            // Move the button to its correct position using a lot of unity code soup
            (button.transform as RectTransform).anchoredPosition = new Vector2((button.transform as RectTransform).anchoredPosition.x, (button.transform as RectTransform).anchoredPosition.y + offset);
            offset -= 50;
        }
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

    public void LoadTuneButton(string buttonArg)
    {
        TuneManager.LoadSetup(buttonArg);
        Cancel();
    }

    public void SaveTune(bool overwrite)
    {
        //when tune save screens save button is pressed - false
        //when tune overwrite button is pressed - true

        //set a listener to the text fields to get all the text. cannot getComponent() text because that shit is 
        string setupNameField = GameObject.Find("SetupNameField").GetComponent<InputField>().text;
        string setupDesc = GameObject.Find("SetupDescField").GetComponent<InputField>().text;

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
        GameObject.Find("QuickCarSelect").GetComponent<Toggle>().isOn = data.QuickCarSelect;
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
        //CAR INSTANTIATE CALL HERE
        carro = Instantiate(carsPrefabs.Find(carpre => carpre.name == fastestLap.Key), Vector3.zero, Quaternion.Euler(0, -90, 15), RecordsCanvas.transform);
        carro.transform.localScale = new Vector3(3, 3, 3);
        carro.transform.localPosition = new Vector3(-0.76f, -0.6f, -922.1f);
        
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
        
        garageCarPrefabName = gameObject.GetComponent<EventSystem>().currentSelectedGameObject.name;
        DataController data = FindObjectOfType<DataController>();
        if (data.QuickCarSelect)
        {
            Debug.Log(gameObject.GetComponent<EventSystem>().currentSelectedGameObject);
            data.SelectedCar = garageCarPrefabName;
            Cancel();
        }
        else
        {
            GarageCanvas.gameObject.SetActive(false);
            GarageTurntableParent.SetActive(true);
            GarageContextCanvas.gameObject.SetActive(false);
            GarageTurntableCanvas.gameObject.SetActive(true);
            FindObjectOfType<Camera>().fieldOfView = 30;
            //get the car, and set some data.
            ContentManager cm = FindObjectOfType<ContentManager>();
            AssetBundle corr = cm.GetCar(garageCarPrefabName);
            TextAsset specs = corr.LoadAsset("specs.json") as TextAsset;
            GarageCarInfo = GarageCarInfo.ImportData(specs.text);//get the car data and cache to a object.
            GameObject.Find("GARAGE_CarName").GetComponent<Text>().text = GarageCarInfo.carName;
            GameObject spawnpoint = new GameObject();

            GameObject spinner = InstantiateCar(corr.name, spawnpoint.transform);
            currentCars.Add(spinner);
            currentCars.Add(spawnpoint);
            spinner.transform.position = GameObject.Find("GARAGE_FLOOR").transform.position;
            spinner.transform.localScale = new Vector3(15, 15, 15);
            // Add rotation script
            spinner.AddComponent<Spinner>();
        }
    }
    
    //select car button
    public void SelectCar()
    {
        GarageOKCanvas.gameObject.SetActive(true);
        Text text = GameObject.Find("GARAGE_OKtext").GetComponent<Text>();
        DataController data = FindObjectOfType<DataController>();
        
        GarageTurntableCanvas.gameObject.SetActive(false);
        if (data.SelectedCar == garageCarPrefabName)
        {
            text.text = "You are already driving this car.";
        }
        else
        {
            text.text = "You are now driving the " + GarageCarInfo.carName + ".";
        }
        data.SelectedCar = garageCarPrefabName;
    }

    public void OpenCarContext()
    {
        string currentCor = GameObject.Find("GARAGE_CarName").GetComponent<Text>().text;
        GarageTurntableCanvas.gameObject.SetActive(false);
        GarageContextCanvas.gameObject.SetActive(true);
        ContentManager cm = FindObjectOfType<ContentManager>();
        GameObject.Find("GARAGE_INFO_TEXT").GetComponent<Text>().text = GarageCarInfo.description;
        FindObjectOfType<Camera>().gameObject.transform.position = GameObject.Find("camPos1").transform.position;
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

    public void Bumsex()
    {
        StartCoroutine(OpenWash());
    }

    IEnumerator OpenWash()
    {
        GoWashCanvas.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        GoRaceCanvas.gameObject.SetActive(false);
        menuMusic.clip = TGmusic[2];
        menuMusic.Play();
        
        DataController dataController = FindObjectOfType<DataController>();
        GameObject spinner = InstantiateCar(dataController.SelectedCar, GoWashCanvas.transform);
        spinner.transform.localScale = new Vector3(90, 90, 90);
        spinner.transform.localPosition = new Vector3(0, -150, -326);
        // Add rotation script
        spinner.AddComponent<Spinner>();
        spinner.AddComponent<CarWash>();
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

    //use this to instantiate cars to spinners/menus. feed it the car name, and a position.
    public GameObject InstantiateCar (string carName, Transform position)
    {
        ContentManager cm = FindObjectOfType<ContentManager>();
        AssetBundle corr = cm.GetCar(carName);
        GameObject nuCar = Instantiate(corr.LoadAsset(carName) as GameObject, position);
        currentCars.Add(nuCar);
        // Disable wheel colliders or unity spergs in the log
        foreach (WheelCollider wc in nuCar.GetComponentsInChildren<WheelCollider>())
        {
            Destroy(wc);
        }
        // Disable main rigidbody
        Destroy(nuCar.GetComponent<Rigidbody>());
        return nuCar;
    }    

    // Save options
    public void SaveOptions() {
        DataController data = FindObjectOfType<DataController>();
        data.QuickCarSelect = GameObject.Find("QuickCarSelect").GetComponent<Toggle>().isOn;
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

            MainMenuMusicCancel();

            foreach (GameObject button in createdGarageButtons) {
                Destroy(button);
            }
            createdGarageButtons.Clear();
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
            MainMenuMusicCancel();
        }
        //Credits -> Go Race
        if(CreditsCanvas.gameObject.activeSelf)
        {
            CreditsCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);
            MainMenuMusicCancel();
        }

        // Licenses -> Go Race
        if (LicensesCanvas.gameObject.activeSelf) {
            dataController.LoadedData.IsBusrider = GameObject.Find("busrider").GetComponent<Toggle>().isOn;

            LicensesCanvas.gameObject.SetActive(false);
            GoRaceCanvas.gameObject.SetActive(true);

            MainMenuMusicCancel();
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
            TuneManager.UnloadButtons();
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

        // Garage Turntable -> Garage
        if (GarageTurntableCanvas.gameObject.activeSelf)
        {
            foreach (GameObject carro in currentCars)
            {
                Destroy(carro);
            }
            GarageContextCanvas.gameObject.SetActive(false);
            GarageTurntableCanvas.gameObject.SetActive(false);
            GarageTurntableParent.SetActive(false);
            GarageCanvas.gameObject.SetActive(true);
            FindObjectOfType<Camera>().fieldOfView = 60;
        }
        
        // Garage Info -> Garage Turntable
        if (GarageContextCanvas.gameObject.activeSelf)
        {

            FindObjectOfType<Camera>().gameObject.transform.position = GameObject.Find("camPos0").transform.position;
            GarageContextCanvas.gameObject.SetActive(false);
            GarageTurntableCanvas.gameObject.SetActive(true);
        }

        // Garage OK -> Garage Turntable
        if (GarageOKCanvas.gameObject.activeSelf)
        {
            GarageOKCanvas.gameObject.SetActive(false);
            GarageTurntableCanvas.gameObject.SetActive(true);
        }

        dataController.SaveGameData();
        yield return null;
    }

    void MainMenuMusicCancel()
    {
        if (System.DateTime.Today.Month == 12)
        {
            menuMusic.clip = TGmusic[4];
        }
        else
        {
            menuMusic.clip = TGmusic[0];
        }
        menuMusic.Play();
    }

}
