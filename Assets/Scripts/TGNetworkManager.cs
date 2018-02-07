using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TGNetworkManager : NetworkManager {
    public List<GameObject> cars;
    public List<GameObject> spawnPositions;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        GameObject player = Instantiate(cars[0], spawnPositions[0].transform.position, Quaternion.identity);
        player.AddComponent<NetworkIdentity>().localPlayerAuthority = true;
        player.AddComponent<NetworkTransform>();
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
