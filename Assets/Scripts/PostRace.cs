using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Networking;

public class PostRace : MonoBehaviour {
    public int laps;
    public int count;
    public GameObject textInstance;
    public List<GameObject> textInstances = new List<GameObject>();
    public Vector2 spawnPos;
    public List<Laptime> lapTally;
    public Canvas mainCanvas;
    public DataController dataController;
    public Laptime personalBest;
    public GameObject[] carPrefabs;
    public GameObject DataControllerPrefab;

    void Start() {
        dataController = FindObjectOfType<DataController>();
        // debug case: create a data controller
        if (dataController == null) {
            dataController = Instantiate(DataControllerPrefab).GetComponent<DataController>();
            dataController.GetComponent<DataController>().LoadGameData();
        }
        // Add laptimes
        if (dataController.RaceResults == null) {
            dataController.RaceResults = new RaceResults() {
                Laptimes = new List<Laptime>(new Laptime[] { TimeSpan.FromSeconds(40), TimeSpan.FromSeconds(61), TimeSpan.FromSeconds(150) }),
            };
        }
        lapTally = dataController.RaceResults.Laptimes;
        laps = lapTally.Count - 1;
        Vector3 spawnPosition = GameObject.Find("spawnPos").transform.position;
        GameObject carro = null;
        carro = Instantiate(carPrefabs.First(carpre => carpre.name == dataController.SelectedCar), spawnPosition, new Quaternion(0, 0, 0, 0));
        CarScriptKill(carro);
        mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        StartCoroutine(TallyLaps());
    }


    public void CarScriptKill(GameObject spinner) {
        // Disable wheel colliders or unity spergs in the log
        foreach (WheelCollider wc in spinner.GetComponentsInChildren<WheelCollider>()) {
            Destroy(wc);
        }
        // Disable car driving scripts
        foreach (Behaviour c in spinner.GetComponents<Behaviour>()) {
            Destroy(c);
        }
        // Disable network scripts
        foreach (Behaviour c in spinner.GetComponents<NetworkTransformChild>()) {
            Destroy(c);
        }
        Destroy(spinner.GetComponent<NetworkTransform>());
        Destroy(spinner.GetComponent<NetworkIdentity>());
        Debug.Log("ignore error, only occurs due to script kill method removing network behavior in wrong order");
        //disable tyre behavior
        foreach (Behaviour c in spinner.GetComponentsInChildren<TireBehavior>()) {
            Destroy(c);
        }
        // Disable main rigidbody
        Destroy(spinner.GetComponent<Rigidbody>());
        //add spinner script
        spinner.AddComponent<Spinner>();
    }

    IEnumerator TallyLaps() {
        yield return new WaitForSeconds(1f);
        //fetch previous personal best for this car
        dataController.BestestLapTimes.TryGetValue(dataController.SelectedCar, out personalBest);
        //if there is no prev personal best,crank dat shit up so it will make a record later in code
        if(personalBest.ms == 0)
        {
            personalBest.ms = 90000;
        }

        //PARSE FROM SUBSTRING TO INT AND TURN TO TIMESPAN
        float raise = 0;//raise next element by x amount
        GameObject despacito;
        foreach (TimeSpan e in lapTally) {
            despacito = null;//clears object reference
            despacito = Instantiate(textInstance, new Vector3(spawnPos.x, spawnPos.y + raise, 0), new Quaternion(0, 0, 0, 0), mainCanvas.transform);
            //create new laptime object/element
            despacito.GetComponent<Text>().text = string.Format("Lap {0}: {1:00}:{2:00}:{3:000}", count + 1, ((TimeSpan)lapTally[count]).Minutes, ((TimeSpan)lapTally[count]).Seconds, ((TimeSpan)lapTally[count]).Milliseconds);
            if (e.Equals(lapTally.Min())) {//bestest lap time
                if (e < personalBest) {
                    despacito.GetComponent<Text>().text += " PB";
                    dataController.BestestLapTimes[dataController.SelectedCar] = new TimeSpan(0, 0, ((TimeSpan)lapTally[count]).Minutes, ((TimeSpan)lapTally[count]).Seconds, ((TimeSpan)lapTally[count]).Milliseconds);
                } else {
                    despacito.GetComponent<Text>().text += " BEST";
                }
                despacito.GetComponent<Text>().color = new Color(200, 0, 0);
            }
            despacito.transform.position = new Vector3(spawnPos.x, spawnPos.y + raise, 0);
            textInstances.Add(despacito);//add all elements to list (will use later)
            raise += 30;//raise by x amount
            count++;//tally/loop
            yield return new WaitForSeconds(0.5f);
        }

        //laptime[count] = GameObject.Find(Convert.ToString(count)).GetComponent<Text>();
        //laptime[count].text = string.Format("{0:00}:{1:00}:{2:000}", lapTally[count].Minutes, lapTally[count].Seconds, lapTally[count].Milliseconds); 

        dataController.SaveGameData();

        yield return new WaitForSecondsRealtime(6);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
        //disabled scene load for easier debugging
        PostRace h = null;
        h = gameObject.AddComponent<PostRace>();
        h.enabled = false;
        h.textInstance = textInstance;
        Destroy(this);
    }
}