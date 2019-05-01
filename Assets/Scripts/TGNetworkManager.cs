using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    // Settings
    public GameData UserSettings;
    // Race Start script
    public RaceStart RaceStart;

    // Players connected on the SERVER, and that stays private
    private Dictionary<NetworkConnection, ConnectedPlayer> Players = new Dictionary<NetworkConnection, ConnectedPlayer>();
    // Player list for the client
    public Dictionary<int, string> ClientPlayers = new Dictionary<int, string>();

    // Called on the HOST after OSC, when the client is connected and ready to be added
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        // Spawn player car
        GameObject player = Instantiate(DefaultPrefab, SpawnPositions[Players.Count].transform.position, Quaternion.identity);

        // Add network identity to base SP prefab
        /*player.AddComponent<NetworkIdentity>().localPlayerAuthority = true;
        player.AddComponent<NetworkTransform>();
        ClientScene.RegisterPrefab(player, NetworkHash128.Parse(player.name + "_" + conn.connectionId));
        spawnPrefabs.Add(player);*/

        // Link connection and prefab
        //NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        // Store player
        Players[conn] = new ConnectedPlayer() { PlayerGO = player, PlayerID = conn.connectionId, PlayerControllerID = playerControllerId, PlayerSpawn = SpawnPositions[Players.Count] };
    }

    // Called on the HOST when the underlying NetworkServer is rady
    public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler(TGMessageTypes.PlayerConnection, HandlePlayerConnection);
        NetworkServer.RegisterHandler(TGMessageTypes.PlayerFinished, HandlePlayerFinished);
    }

    // Called on the HOST when a player leaves
    public override void OnServerDisconnect(NetworkConnection conn) {
        base.OnServerDisconnect(conn);
        ConnectedPlayer player = Players[conn];
        Destroy(player.PlayerGO);
        Players.Remove(conn);

        // Update UI
        string newText = "Players: " + Players.Count + "\n";
        foreach (ConnectedPlayer knownPlayer in Players.Values) {
            newText += knownPlayer.PlayerName + "\n";
        }
        GameObject.Find("PlayerList").GetComponent<Text>().text = newText;
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
            if (prefab.name == currentPlayer.CarName) {
                GameObject carInstance = Instantiate(prefab, currentPlayer.PlayerSpawn.transform.position, Quaternion.identity);
                currentPlayer.PlayerGO = carInstance;
                NetworkServer.AddPlayerForConnection(rawMessage.conn, carInstance, currentPlayer.PlayerControllerID);
                carInstance.GetComponent<CheckpointReader>().username = currentPlayer.PlayerName;
                break;
            }
        }

        // Update UI
        string newText = "Players: " + Players.Count + "\n";
        foreach (ConnectedPlayer knownPlayer in Players.Values) {
            newText += knownPlayer.PlayerName + "\n";
        }
        GameObject.Find("PlayerList").GetComponent<Text>().text = newText;

        // Send player list
        List<int> ids = new List<int>();
        List<string> names = new List<string>();
        foreach (var kvp in Players.Values) {
            ids.Add(kvp.PlayerID);
            names.Add(kvp.PlayerName);
        }
        PlayerListMessage playerListMessage = new PlayerListMessage() {
            NetworkID = ids.ToArray(),
            PlayerNames = names.ToArray()
        };

        foreach (var connectedPlayer in Players.Keys) {
            connectedPlayer.Send(TGMessageTypes.PlayerList, playerListMessage);
        }
    }

    // Called on the HOST to handle a player finishing the race, starting end of race procedure
    public void HandlePlayerFinished(NetworkMessage netMsg) {
        foreach(var conn in Players.Keys) {
            conn.Send(TGMessageTypes.RaceEnd, new RaceEndMessage());
        }
    }

    // Called on the CLIENT to handle sending the PlayerConnection message
    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        client.RegisterHandler(TGMessageTypes.CountdownStart, HandleClientCountdownStart);
        client.RegisterHandler(TGMessageTypes.PlayerList, HandlePlayerList);
        client.RegisterHandler(TGMessageTypes.RaceEnd, HandleRaceEnd);

        // Register the event to be used by RaceStart to send the player done message
        RaceStart.PlayerFinished += RaceStart_PlayerFinished;

        client.Send(TGMessageTypes.PlayerConnection, new PlayerConnectionMessage() { CarName = UserSettings.SelectedCar, PlayerName = UserSettings.PlayerName });
    }

    // Called on the CLIENT by RaceStart (NOT NET) to send race done message
    private void RaceStart_PlayerFinished() {
        client.Send(TGMessageTypes.PlayerFinished, new PlayerFinishedMessage());
    }

    // Called on the CLIENT to handle the start of the countdown
    private void HandleClientCountdownStart(NetworkMessage netMsg) {
        CountdownStartMessage message = netMsg.ReadMessage<CountdownStartMessage>();
        RaceStart.enabled = true;
        RaceStart.StartRace(message.GridSpot);
    }


    // event to trigger player list refresh
    public delegate void PlayerListRefresh(Dictionary<int, string> Players);
    public static event PlayerListRefresh PlayerListRefreshEvent;

    // Called on the CLIENT to handle receiving player list from server
    private void HandlePlayerList(NetworkMessage netMsg) {
        PlayerListMessage message = netMsg.ReadMessage<PlayerListMessage>();
        ClientPlayers.Clear();
        for(int i = 0; i < message.PlayerNames.Length; i++) {
            ClientPlayers.Add(message.NetworkID[i], message.PlayerNames[i]);
        }
        PlayerListRefreshEvent.Invoke(ClientPlayers);
    }

    // Called on the CLIENT to handle race end
    private void HandleRaceEnd(NetworkMessage netMsg) {
        RaceStart.EndRaceWrapper();
    }

    // Called on the SERVER when the host starts the game
    public void StartRaceProcess() {
        // RANDOM GRID BOYO
        System.Random rnd = new System.Random();
        int[] positions = Enumerable.Range(1, Players.Count).OrderBy(x => rnd.Next()).ToArray();

        int i = 0;
        foreach(NetworkConnection player in Players.Keys) {
            player.Send(TGMessageTypes.CountdownStart, new CountdownStartMessage() { PlayersConnected = Players.Count, GridSpot = positions[i] });
            i++;
        }
    }


    // Network message classes
    public class TGMessageTypes {
        public static short PlayerConnection = MsgType.Highest + 1;
        public static short CountdownStart = MsgType.Highest + 2;
        public static short PlayerList = MsgType.Highest + 3;
        public static short RaceEnd = MsgType.Highest + 4;
        public static short PlayerFinished = MsgType.Highest + 4;
    };

    public class PlayerConnectionMessage : MessageBase {
        public string CarName;
        public string PlayerName;
    }

    public class CountdownStartMessage : MessageBase {
        public int PlayersConnected;
        public int GridSpot;
    }

    public class PlayerListMessage : MessageBase {
        public string[] PlayerNames;
        public int[] NetworkID;
    }

    public class RaceEndMessage : MessageBase {

    }

    public class PlayerFinishedMessage : MessageBase {

    }

    // Player state class
    public class ConnectedPlayer {
        public string CarName;
        public string PlayerName;
        public GameObject PlayerGO;
        public int PlayerID;
        public short PlayerControllerID;
        internal GameObject PlayerSpawn;
    }
}
