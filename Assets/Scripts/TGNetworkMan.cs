using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.UI;

public class TGNetworkMan : MonoBehaviour
{
    public bool debugMode = false;//enable this for possible debug stuff
    //public DebugUIMan debugUI;
    
    public SessionInfo session;
    public DataController data;
    public TGnetworkUDP network;

    public string IP = "127.0.0.1";
    public int UDPport = 11111;
    public int TCPport = 11112;

    public bool isServer = false;


    public bool isRunning = false;//indicator light
    public List<string> textMessages = new List<string>();

    int UDPtrafficTimer = 0;//container
    

    private void Start()
    {
        session = new SessionInfo();
        network = new TGnetworkUDP();
        data = FindObjectOfType<DataController>();
        
        IP = data.IP;

        //you are the host
        if (IP == "")
        {
            isServer = true;
            isRunning = true;

            Debug.Log("starting server");
            network.InitiateUDPReader();
            NetPlayer dolor = new NetPlayer();
            dolor.username = dolor.TooLongUsernameReplacer();
            dolor.carname = data.SelectedCar;
            dolor.token = "ttt";
            dolor.vehicle = gameObject;
            session.playerInfo.Add(dolor);
            Debug.Log("reached end of start() " + dolor.DataAsString());
        }
        else
        {
            network.serverAddress = new IPEndPoint(IPAddress.Parse(IP), UDPport);
            //start client
            Debug.Log(IPAddress.Parse(IP).ToString() + network.serverAddress.Address.ToString());

            isRunning = true;
            network.InitiateUDPReader();
            
            NetPlayer dolor = new NetPlayer();
            //make local player
            dolor.username = dolor.TooLongUsernameReplacer();
            dolor.carname = data.SelectedCar;
            dolor.token = "null";
            session.playerInfo.Add(dolor);
            
            //send info to server
            network.SendStringUDP("0|" + dolor.DataAsString(), network.serverAddress);
            
            
            //receive token from server
            Debug.Log("packet sent AMOGUS");
            CommandReader(network.ReceiveUDPasString());//SocketException: An existing connection was forcibly closed by the remote host.
            Debug.Log("reached end of start() " + dolor.DataAsString());
        }
    }

    private void FixedUpdate()
    {
        UDPtrafficTimer++;
        if(UDPtrafficTimer > network.UDPpacketSendRate)
        {
            UDPtrafficTimer = 0;
            if (isServer)
            {
                Debug.Log("scanning for packets");
                CommandReader(network.ReceiveUDPasString());

            }
            
            
            //RefreshPos();
            

        }


    }

    void RefreshPos()
    {
        network.SendStringUDP("3" + session.playerInfo[0].token + "COCK AND BALLL TORTURE", network.serverAddress);


    }

    //use this to make sure there arent 2 players with the same token..just for the off chance
    //with my luck, this will totally happen so i made this
    bool CheckDuplicateToken(string token)
    {
        foreach(NetPlayer culo in session.playerInfo)
        {
            if(culo.token == token)
            {
                //duplicate, do not break loop.
                return false;
            }
        }
        //no duplicates found, carry on
        return true;
    }

    //to make network traffic quick n easy
    //give different actions a code
    //and handle them
    void CommandReader(TGnetworkUDP.Packet packet)
    {
        //0 - handshake
        //1 - add player
        //2 - text message
        //3 - updatePosition

        string commandType = packet.command.Substring(0, 1);
        switch (commandType)
        {
            case "0":
                UDPhandshake(packet);
                break;
            case "1":
                AddPlayer(packet.command);
                break;
            case "2":
                TextChatRefreshMessages(packet.command);
                //parse pos and apply


                break;
        }
    }

    public void UDPhandshake(TGnetworkUDP.Packet packet)
    {
        //if server, send client their token
        //then send every other session users info.
        if (isServer)
        {
            NetPlayer samir = new NetPlayer();
            //data = "0|" + username + "|" + carname + "|" + token + "|" + ip address;
            samir.ImportPlayerData(packet.command);
            samir.address = packet.origin;
            while (true)
            {
                //generate tokens until you get a unique token
                samir.token = samir.GenerateUDPToken();
                if (CheckDuplicateToken(samir.token)) break;
            }
            Debug.Log("token generated.sending...");
            network.SendStringUDP("0|" + samir.token, samir.address);
            //after sending token to joining player, add joining player via joinplayer
            //that broadscasts the join event.
            string addPlayerCommand = "1|" + samir.DataAsString();

            //broadcast it to everybody connected.

            for(int i = 1; i < session.playerInfo.Count; i++)
            {
                network.SendStringUDP(addPlayerCommand, session.playerInfo[i].address);
                //iterate through everyone n send new players info.
                //dont tell myself that a player joined i already know you fuckin buffoon
            }
            session.playerInfo.Add(samir);
            //then instantiate samir to server
            Debug.Log("instantiate samir please sir; " + addPlayerCommand);
        }
        else
        {
            string[] command = packet.command.Split();
            session.playerInfo[0].token = command[1];
        }

    }
    void AddPlayer(string data)
    {
        //host announces a new user. tally em up.
        NetPlayer venkatesh = new NetPlayer();
        venkatesh.ImportPlayerData(data);
        //instantiate venkatesh's game object
        Debug.Log("good sir please... instantiate venkatesh i begging u; " + data);
    }

    
    void TextChatRefreshMessages(string incomingMessage)
    {
        




    }
}

public class TGnetworkUDP
{
    public Socket client;
    public IPEndPoint serverAddress;
    int port = 11111;
    public List<IPEndPoint> connectedClients = new List<IPEndPoint>();
    public int UDPpacketSendRate = 10;//send packet every x ticks

    public struct Packet
    {
       public string command;
       public IPEndPoint origin;
    }

    public Packet ReceiveUDPasString()
    {
        EndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
        byte[] data = new byte[256];
        client.ReceiveFrom(data, ref RemoteIpEndPoint);
        Packet content = new Packet();
        content.command = Encoding.UTF8.GetString(data);
        content.origin = (IPEndPoint)RemoteIpEndPoint;
        return content;
    }

    public void SendStringUDP(string content, IPEndPoint destination)
    {
        byte[] data = new byte[content.Length];
        data = Encoding.UTF8.GetBytes(content);
        Debug.Log(destination.Address.ToString());
        client.SendTo(data, destination);
    }
    public void InitiateUDPReader()
    {
        this.client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        this.client.ReceiveTimeout = 100;
        this.client.SendTimeout = 100;

        Debug.Log("UDP server open");
    }
}


public class TGnetworkTCP
{
    public TGserverTCP TCPserver;
    public TGclientTCP TCPclient;
    
    
    string ReceiveTCPasString(Socket TCPsock, int expectedDataLength)
    {
        byte[] data = new byte[expectedDataLength];
        TCPsock.Receive(data);
        string content = Encoding.UTF8.GetString(data);
        content = content.Trim('\0');//make sure u cull out empty bytes from the end of the string
        //or funny things will happen.
        return content;
    }

    //give a tcp socket and a string and it will send the string as bytes
    void SendStringTCP(string content, Socket TCPsock)
    {
        byte[] data = new byte[content.Length];
        data = Encoding.UTF8.GetBytes(content);
        TCPsock.Send(data);
    }

    //server specific TCP things
    public class TGserverTCP
    {
        Socket TCPListenerSocket;
        int port = 11112;
        bool isListenerOn;

        //called from Start() - SERVER: starts listener socket.
        private void InitListenerSocket(int port)
        {
            this.port = port;
            //create socket
            TCPListenerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            //bind the listening socket to the port
            IPEndPoint endpoint = new IPEndPoint(IPAddress.IPv6Any, port);
            TCPListenerSocket.Bind(endpoint);
            Debug.Log("tcp listener bound.");
            //start listening
            TCPListenerSocket.Listen(4);
            //delegate listening to a thread
            Thread cbt = new Thread(new ThreadStart(TCPlistenerThread));
            cbt.Start();
            isListenerOn = true;
        }

        //this is the listener thread where you llisten n tally up incoming TCP connections.
        private void TCPlistenerThread()
        {
            Debug.Log("initiating listener thread");
            do
            {
                Socket perro = TCPListenerSocket.Accept();//accept incoming client to a new socket object
                IPEndPoint perroEP = perro.RemoteEndPoint as IPEndPoint;//get da IP address for da prompt
                Debug.Log("el nuevo connecion: " + perroEP.Address.ToString());
                JoinHandshake(perro);//handshake handler for joining user

            } while (true);
            isListenerOn = false;
        }

        //SERVER: TCP HANDSHAKE / TRADE SESSION INFO
        void JoinHandshake(Socket tcpsock)//WIP WIP WIP WIP WIP WIP
        {
            NetPlayer samir = new NetPlayer();//create new netplayer
            samir.TCPsock = tcpsock;//bundle the relevant sock with this player

            //string handshakeInfo = ReceiveTCPasString(tcpsock, 4096);
            //just get 4k bytes i think thats plenty of characters for a funny username + game data.
            //make the client vet the name so theres enough bytes for the rest of the data.

            //samir.ImportPlayerData(handshakeInfo);//load the data into the new player object
            //textMessages.Add(handshakeInfo);//DEBUG
            Debug.Log("received user " + samir.username + " info. sending token...");

            //send token to joining client
            //SendStringTCP(samir.token, samir.TCPsock);

            //after this, send rest of the session info. like connected players, time of day etc
            //then write something to do UDP back n forth...
            //session.playerInfo.Add(samir);//everything went well, add player to player list
            Debug.Log("everything worked.very nice");
            
        }

    }

    //client
    public class TGclientTCP
    {
        //stole this from microsofts doc so it should work no problem.
        //connect to a remote host
        Socket ConnectSocket(string IP, int TCPport)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;

            //Get host related information.
            //GETTING LOCALHOST VIA THIS METHOD RETURNS IPV6!!!!!!!!!!!!
            hostEntry = Dns.GetHostEntry(IP);

            //Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            //an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            Debug.Log("attempting connectiong to : " + IP);
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, TCPport);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            Debug.Log("TCP socket connected to: " + IP);
            return s;
            //proceed to handshake
        }
        
        //send this users info to server. then receive the session info from server.
        void JoinHandshake(Socket tcpsock)
        {
            //get relevant data from datacontroller
            NetPlayer myself = new NetPlayer();
            myself.username = myself.TooLongUsernameReplacer();//using this temporaily because
                                                               //i think its cool
            //just make sure its not too long
            //TODO: vet other data too, maybe make a separate bool/void for it i guess..
            if (myself.username.Length > 2000)
            {
                myself.username = myself.TooLongUsernameReplacer();
            }

            myself.carname = "stargt";
            myself.token = "null";
            
            //send server your info, and server replies with your token.

            Debug.Log(myself.DataAsString());
            //very cool if it works this far.
        }
    }
}



public class SessionInfo
{
    public string sessionPass = "";
    public List<NetPlayer> playerInfo = new List<NetPlayer>();
    public NetUI networkUI;
    


    public string PlayerNameFromToken(string token)
    {
        foreach(NetPlayer gamer in playerInfo)
        {
            if(gamer.token == token)
            {
                return gamer.username;
            }
        }
        //no user found with that token
        return "BAD NEWS BROWN";
    }



    public class NetUI
    {
        public Canvas TextChatUI;
        public List<Text> TextMessages;

        public Canvas PlayerListUI;
        public List<Text> Playernames;

    }
}

public class NetPlayer
{
    public GameObject vehicle;//the instantiated physical car
    public string username;//screem name
    public string carname;//car CONTENT MANAGER NAME
    public string token;//token used in UDP transmission regarding this instance
    public Socket TCPsock;//used by TCP. pointer to this instances TCPsocket (if TCP is used)
    public IPEndPoint address;//used by UDP to send packets to correct destinations.


    //spit out user data as delimited string.
    public string DataAsString()
    {
        string data;
        data = username + "|" + carname + "|" + token + "";

        return data;
    }

    //import a NetPlayer object from a string, applies to THIS object
    public void ImportPlayerData(string data)
    {
        string[] splicedData = data.Split('|');
        username = splicedData[1];
        carname = splicedData[2];
        token = splicedData[3];
    }

    //used to generate UDP token. UDP token is 3 letters long.
    public string GenerateUDPToken()
    {
        string validKeys = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string genToken = "";

        System.Random dolor = new System.Random();

        while (genToken.Length < 4)
        {
            genToken += validKeys.Substring(dolor.Next(0, validKeys.Length - 1), 1);
        }
        return genToken;
    }

    //username so long it breaks the arbitary character limit that i will mindlessly adjust
    public string TooLongUsernameReplacer()
    {
        string[] happyFriendsWords = new string[] { "3am", "1nsain", "Samir", "CIA", "Happy", "Friend", "Burger", "Enjoyer", "Glowie", "Afongus", "Frenchman", "Top", "Earth", "Shift", "Brazilian"};
        System.Random dolor = new System.Random();
        int length = dolor.Next(1, 4);
        string cooltext = "";
        for(int i = 0; i < length; i++)
        {
            cooltext += happyFriendsWords[dolor.Next(0, happyFriendsWords.Length - 1)];
        }
        return cooltext;
    }


}