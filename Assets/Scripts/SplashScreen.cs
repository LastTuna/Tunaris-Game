using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour {
    void Start() {
        StartCoroutine(LoadMainMenu());
    }

	IEnumerator LoadMainMenu() {
        yield return new WaitForSeconds(5);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("main_menu");
        while (!asyncLoad.isDone) {
            yield return null;
        }
    }
}
