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
    
    public List<NetPlayer> playerInfo = new List<NetPlayer>();
    
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
        DataController data = FindObjectOfType<DataController>();
        IP = data.IP;

        //you are the host
        if(IP == "")
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
            JoinGameServerHandler(perro);// WIP WIP WIP WIP

        } while (isRunning);
    }
    
    void JoinGameServerHandler(Socket tcpsock)//WIP WIP WIP WIP WIP WIP
    {
        NetPlayer samir = new NetPlayer();
        samir.TCPsock = tcpsock;
        textMessages.Add(ReceiveTCPasString(tcpsock));
        playerInfo.Add(samir);
    }

    void JoinGameClientHandler(Socket tcpsock)
    {
        SendStringTCP("bigfloyd88", tcpsock);
    }

    //UNTESTED
    string ReceiveTCPasString(Socket TCPsock)
    {
        byte[] data = new byte[128];
        TCPsock.Receive(data);
        string content = Encoding.UTF8.GetString(data);
        return content;
    }
    //UNTESTED
    void SendStringTCP(string content, Socket TCPsock)
    {
        byte[] data = new byte[128];
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
        JoinGameClientHandler(s);
    }


}

public class SessionInfo
{
    






}

public class NetPlayer
{
    public GameObject vehicle;//the instantiated physical car
    public string username;//screem name
    public string carname;//car CONTENT MANAGER NAME
    public string token;//token used in UDP transmission regarding this instance
    public Socket TCPsock;//used by server-pointer to this instances socket




}