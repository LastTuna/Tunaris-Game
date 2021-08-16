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
    
    Socket TCPSocket;
    public List<Socket> TCPConnections = new List<Socket>();

    public string IP = "127.0.0.1";
    public int UDPport = 11111;
    public int TCPport = 11112;
    public string clientMsg = "";//write a message here and press enter while client is running to send a messag to server
    
    public bool isRunning = false;//indicator light
    public List<string> textMessages = new List<string>();
    

    //the comments are in german because i stole half of this shit from a stackoverflow.
    //i have no idea what half of the shit means and im too lazy to delete it all.
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
            isRunning = true;
            TCPSocket = ConnectSocket(IP, TCPport);
            TCPSocket.Disconnect(false);
            TCPSocket.Close();
            //start client
        }
    }
    
    private void InitListenerSocket()
    {
        //create socket
        TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //bind the listening socket to the port
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, TCPport);
        TCPSocket.Bind(endpoint);

        //start listening
        TCPSocket.Listen(4);

        Thread cbt = new Thread(new ThreadStart(Insain));
        cbt.Start();
    }

    //this is the listener thread where you llisten n tally up incoming TCP connections.
    private void Insain()
    {
        do
        {
            Socket perro = TCPSocket.Accept();
            TCPConnections.Add(perro);
            IPEndPoint perroEP = perro.RemoteEndPoint as IPEndPoint;//get da IP address for da prompt
            Debug.Log("el nuevo connecion: " + perroEP.Address.ToString());
        } while (isRunning);
    }


    //stole this from microsofts doc so it should work no problem.
    private static Socket ConnectSocket(string serverIP, int port)
    {
        Socket s = null;
        IPHostEntry hostEntry = null;

        //Get host related information.
        hostEntry = Dns.GetHostEntry(serverIP);

        //Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
        //an exception that occurs when the host IP Address is not compatible with the address family
        // (typical in the IPv6 case).
        Debug.Log("attempting connectiong to : " + serverIP);
        foreach (IPAddress address in hostEntry.AddressList)
        {
            IPEndPoint ipe = new IPEndPoint(address, port);
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
        Debug.Log("TCP socket connected to: " + serverIP);
        return s;
    }


}


public class NetPlayer
{




}