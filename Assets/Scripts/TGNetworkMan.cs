using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class TGNetworkMan : MonoBehaviour
{
    public SessionInfo session;
    public DataController data;
    Socket TCPListenerSocket;//listener sock used by server
    Socket TCPSocket;//used by client

    public string IP = "127.0.0.1";
    public int UDPport = 11111;
    public int TCPport = 11112;
    public string clientMsg = "";//write a message here and press enter while client is running to send a messag to server

    public bool isRunning = false;//indicator light
    public List<string> textMessages = new List<string>();


    private void Start()
    {
        session = new SessionInfo();
        data = FindObjectOfType<DataController>();
        IP = data.IP;

        //you are the host
        if (IP == "")
        {
            isRunning = true;
            Debug.Log("starting server");
            InitListenerSocket();
        }
        else
        {
            //start client
            isRunning = true;
            ConnectSocket();
        }
    }

    //called from Start() - SERVER: starts listener socket.
    private void InitListenerSocket()
    {
        //create socket
        TCPListenerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        //bind the listening socket to the port
        IPEndPoint endpoint = new IPEndPoint(IPAddress.IPv6Any, TCPport);
        TCPListenerSocket.Bind(endpoint);
        Debug.Log("tcp listener bound.");
        //start listening
        TCPListenerSocket.Listen(4);
        //delegate listening to a thread
        Thread cbt = new Thread(new ThreadStart(TCPlistenerThread));
        cbt.Start();
    }

    //this is the listener thread where you llisten n tally up incoming TCP connections.
    private void TCPlistenerThread()
    {

        Debug.Log("initiating listener thread");
        do
        {
            Socket perro = TCPListenerSocket.Accept();
            IPEndPoint perroEP = perro.RemoteEndPoint as IPEndPoint;//get da IP address for da prompt
            Debug.Log("el nuevo connecion: " + perroEP.Address.ToString());
            JoinGameServerHandshake(perro);//handshake handler for joining user

        } while (isRunning);
    }

    //SERVER: TCP HANDSHAKE / TRADE SESSION INFO
    void JoinGameServerHandshake(Socket tcpsock)//WIP WIP WIP WIP WIP WIP
    {
        bool greenflag = false;//true = password is correct

        //check if the joining client gives correct password. if password is empty, skip check.
        if (session.sessionPass != "")
        {
            if (ReceiveTCPasString(tcpsock, session.sessionPass.Length) == session.sessionPass)
            {
                greenflag = true;//password is correct
                Debug.Log("password was correct");
            }
        }

        //if password is blank, execute this. if password is correct, execute
        if (session.sessionPass == "" || greenflag)
        {
            NetPlayer samir = new NetPlayer();//create new netplayer
            samir.TCPsock = tcpsock;//bundle the relevant sock with this player
            
            string handshakeInfo = ReceiveTCPasString(tcpsock, 4096);
            //just get 4k bytes i think thats plenty of characters for a funny username + game data.
            //make the client vet the name so theres enough bytes for the rest of the data.

            samir.ImportPlayerData(handshakeInfo);//load the data into the new player object
            textMessages.Add(handshakeInfo);//DEBUG
            Debug.Log("received user " + samir.username + " info. sending token...");

            while (true)
            {
                //generate tokens until you get a unique token
                samir.token = samir.GenerateUDPToken();
                if (CheckDuplicateToken(samir.token)) break;
            }
            Debug.Log("token generated.sending...");
            //send token to joining client
            SendStringTCP(samir.token, samir.TCPsock);

            //after this, send rest of the session info. like connected players, time of day etc
            //then write something to do UDP back n forth...
            session.playerInfo.Add(samir);//everything went well, add player to player list
            Debug.Log("everything worked.very nice");

        }
        else
        {
            //kill connection if wrong pass
            Debug.Log("wrong password, timing out...");
            tcpsock.Disconnect(false);
            tcpsock.Close();
        }
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

    //WIP WIP WIP WIP WIP WIP WIP
    //send this users info to server. then receive the session info from server.
    void JoinGameClientHandshake(Socket tcpsock)
    {
        //get relevant data from datacontroller
        NetPlayer myself = new NetPlayer();
        myself.username = myself.TooLongUsernameReplacer();//using this temporaily because
        //i think its cool


        //just make sure its not too long
        //TODO: vet other data too, maybe make a separate bool/void for it i guess..
        if(myself.username.Length > 2000)
        {
            myself.username = myself.TooLongUsernameReplacer();
        }
        
        myself.carname = data.SelectedCar;
        myself.token = "null";
        session.sessionPass = "";
        
        //TODO ADD SERVER PASSWORD FIELD TO SAVEDATA or add it as join param somewhere? i dunno yet...

        if (session.sessionPass != "") SendStringTCP(session.sessionPass, tcpsock);
        //send password if there is one

        //carry on, send things about myself
        SendStringTCP(myself.DataAsString(), tcpsock);

        myself.token = ReceiveTCPasString(tcpsock, 3);
        Debug.Log(myself.DataAsString());
        //very cool if it works this far.
    }

    //receive TCP data and convert to string.
    //in some cases, we dont want to read any other random shit off the buffer.. so add byte length there
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

    //stole this from microsofts doc so it should work no problem.
    private void ConnectSocket()
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
        TCPSocket = s;
        Debug.Log("TCP socket connected to: " + IP);
        JoinGameClientHandshake(s);
        //proceed to handshake
    }


}

public class SessionInfo
{
    public string sessionPass = "";
    public List<NetPlayer> playerInfo = new List<NetPlayer>();

    



}

public class NetPlayer
{
    public GameObject vehicle;//the instantiated physical car
    public string username;//screem name
    public string carname;//car CONTENT MANAGER NAME
    public string token;//token used in UDP transmission regarding this instance
    public Socket TCPsock;//used by server-pointer to this instances socket


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
        username = splicedData[0];
        carname = splicedData[1];
        token = splicedData[2];
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