using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TGNetworkManager : NetworkManager {
    // Cars prefabs
    public List<GameObject> Cars;
    // Spawn positions for that level
    public List<GameObject> SpawnPositions;
    // Default player prefab
    public GameObject DefaultPrefab;

    // Players connected
    public Dictionary<NetworkConnection, ConnectedPlayer> Players = new Dictionary<NetworkConnection, ConnectedPlayer>();

    // Called on the HOST after OSC, when the client is connected and ready to be added
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        // Spawn player car
        GameObject player = Instantiate(DefaultPrefab, SpawnPositions[Players.Count].transform.position, Quaternion.identity);

        // Add network identity to base SP prefab
        player.AddComponent<NetworkIdentity>().localPlayerAuthority = true;
        player.AddComponent<NetworkTransform>();

        // Link connection and prefab
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        // Store player
        Players[conn] = new ConnectedPlayer() { PlayerGO = player };
    }

    // Called on the HOST when the underlying NetworkServer is rady
    public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler(TGMessageTypes.PlayerConnection, HandlePlayerConnection);
    }

    // Called on the HOST when a player leaves
    public override void OnServerDisconnect(NetworkConnection conn) {
        base.OnServerDisconnect(conn);
        ConnectedPlayer player = Players[conn];
        Destroy(player.PlayerGO);
        Players.Remove(conn);
    }

    // Called on the HOST to handle the PlayerConnection message from the client
    // This message is sent right after the client connects in XXX
    private void HandlePlayerConnection(NetworkMessage rawMessage) {
        // Parse the message back
        PlayerConnectionMessage currentPlayerMessage = rawMessage.ReadMessage<PlayerConnectionMessage>();

        // Retreive all the data
        ConnectedPlayer currentPlayer = Players[rawMessage.conn];
        currentPlayer.PlayerName = currentPlayerMessage.PlayerName;
        currentPlayer.CarName = currentPlayerMessage.CarName;

        // Instantiate the actual car
        foreach(GameObject prefab in Cars) {
            Debug.Log("_" + prefab.name +  "__" + currentPlayer.CarName + "_");
            if (prefab.name == currentPlayer.CarName) {
                Instantiate(prefab, currentPlayer.PlayerGO.transform);
                break;
            }
        }

        // Update UI
        string newText = "Players: " + Players.Count + "\n";
        foreach (ConnectedPlayer knownPlayer in Players.Values) {
            newText += knownPlayer.PlayerName + "\n";
        }
        GameObject.Find("PlayerList").GetComponent<Text>().text = newText;
    }

    // Called on the CLIENT to handle sending the PlayerConnection message
    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        client.Send(TGMessageTypes.PlayerConnection, new PlayerConnectionMessage() { CarName = "ralllyx", PlayerName = "bite" });
    }

    // Network message classes
    public class TGMessageTypes {
        public static short PlayerConnection = MsgType.Highest + 1;
    };

    public class PlayerConnectionMessage : MessageBase {
        public string CarName;
        public string PlayerName;
    }

    // Player state class
    public class ConnectedPlayer {
        public string CarName;
        public string PlayerName;
        public GameObject PlayerGO;
    }
}
