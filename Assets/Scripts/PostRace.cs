using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Networking;

public class PostRace : MonoBehaviour
{
    public bool checkum;
    public int laps;
    public int count;
    public int carIndex;
    public GameObject textInstance;
    public List<GameObject> textInstances = new List<GameObject>();
    public Vector2 spawnPos;
    public List<TimeSpan> lapTally = new List<TimeSpan>();
    public Canvas mainCanvas;
    public DataController dataController;
    public TimeSpan personalBest;
    public GameObject[] carPrefabs;

    void Start()
    {
        dataController = FindObjectOfType<DataController>();
        checkum = true;
        lapTally = GameObject.Find("CourseController").GetComponent<RaceStart>().laptimes;
        laps = lapTally.Count - 1;
        carIndex = FindObjectOfType<CarBehaviour>().carIndex;

    }


    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("0") && checkum)
        {
            Vector3 spawnPosition = GameObject.Find("spawnPos").transform.position;
            GameObject carro = null;
            carro = Instantiate(carPrefabs[carIndex], spawnPosition, new Quaternion(0, 0, 0, 0));
            CarScriptKill(carro);
            mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            StartCoroutine(TallyLaps());
        }
    }

    public void CarScriptKill(GameObject spinner)
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
        //add spinner script
        spinner.AddComponent<Spinner>();
    }

    IEnumerator TallyLaps()
    {
        personalBest = new TimeSpan(0,0,
            Convert.ToInt32(dataController.BestestLapTimes[0].Substring(0, 2)),
            Convert.ToInt32(dataController.BestestLapTimes[0].Substring(3, 2)),
            Convert.ToInt32(dataController.BestestLapTimes[0].Substring(6, 3)));

        //PARSE FROM SUBSTRING TO INT AND TURN TO TIMESPAN
        checkum = false;
        float raise = 0;//raise next element by x amount
        GameObject despacito;
        foreach (TimeSpan e in lapTally)
        {
            despacito = null;//clears object reference
            despacito = Instantiate(textInstance, new Vector3(spawnPos.x, spawnPos.y + raise, 0), new Quaternion(0, 0, 0, 0), mainCanvas.transform);
            //create new laptime object/element
            despacito.GetComponent<Text>().text = string.Format("Lap {0}: {1:00}:{2:00}:{3:000}", count + 1, lapTally[count].Minutes, lapTally[count].Seconds, lapTally[count].Milliseconds);
            if (e.Equals(lapTally.Min()))
            {//bestest lap time
                if (e < personalBest)
                {
                    despacito.GetComponent<Text>().text += " PB";
                    dataController.BestestLapTimes[0] = string.Format("{0:00}:{1:00}.{2:000}_{3}", lapTally[count].Minutes, lapTally[count].Seconds, lapTally[count].Milliseconds, carIndex);
                }
                else
                {
                    despacito.GetComponent<Text>().text += " BEST";
                }
                despacito.GetComponent<Text>().color = new Color(200,0,0);
            }
            despacito.transform.position = new Vector3(spawnPos.x, spawnPos.y + raise, 0);
            textInstances.Add(despacito);//add all elements to list (will use later)
            raise += 30;//raise by x amount
            count++;//tally/loop
            yield return new WaitForSeconds(0.1f);
            Debug.Log(count);
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