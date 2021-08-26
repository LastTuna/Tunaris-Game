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
        DataController data = FindObjectOfType<DataController>();
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
            Thread tt = new Thread(new ThreadStart(ConnectSocket));
            tt.Start();
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
            session.playerInfo.Add(samir);//everything went well, add player to player list
            textMessages.Add(handshakeInfo);//DEBUG
            Debug.Log("received");

            //WRITE CODE TO SEND SERVER INFO TO CLIENT.

        }
        else
        {
            //kill connection if wrong pass
            Debug.Log("wrong password, timing out...");
            tcpsock.Disconnect(false);
            tcpsock.Close();
        }
    }

    //WIP WIP WIP WIP WIP WIP WIP
    //send this users info to server. then receive the session info from server.
    void JoinGameClientHandshake(Socket tcpsock)
    {
        //test shit for now
        NetPlayer test = new NetPlayer();
        test.username = "What the fuck did you just fucking say about me, you little bitch? I'll have you know I graduated top of my class in the Navy Seals, and I've been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I'm the top sniper in the entire US armed forces. You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. You're fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that's just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little \"clever\" comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn't, you didn't, and now you're paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You're fucking dead, kiddo.";
        test.carname = "stargt";
        test.token = "microwave";
        SendStringTCP(test.DataAsString(), tcpsock);
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
}