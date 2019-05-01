using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListScript : MonoBehaviour {
    public GameObject PlayerListCanvas;
    public GameObject PlayerListItemPrefab;
    public GameObject InstantiateReference;

    void Start() {
        TGNetworkManager.PlayerListRefreshEvent += TGNetworkManager_PlayerListRefreshEvent;
    }

    private void TGNetworkManager_PlayerListRefreshEvent(Dictionary<int, string> Players) {
        Debug.Log("PlayerListRefresh Event");
        if (PlayerListCanvas != null) {
            // Delete previous list
            foreach (Transform childplrname in PlayerListCanvas.transform) {
                if (!childplrname.gameObject.name.StartsWith("_")) {
                    Destroy(childplrname.gameObject);
                }
            }
            // Build new list
            Vector3 instantiatePosition = InstantiateReference.transform.position;
            instantiatePosition.y -= 30;
            foreach (string playername in Players.Values) {
                var newgo = Instantiate(PlayerListItemPrefab, instantiatePosition, Quaternion.identity, PlayerListCanvas.transform);
                instantiatePosition.y -= 30;
                newgo.GetComponent<Text>().text = playername;
            }
        }
    }

    void Update() {
        PlayerListCanvas.SetActive(Input.GetButton("PlayerList"));
    }
}
