using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TGNetworkMan : MonoBehaviour
{

    // use this thread for whatever.
    //server/client uses this same object just to make sure u cant fat finger both server and client on at the same time
    Thread Thread;

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    public string IP = "127.0.0.1";
    public int port = 11111;//port that server and cleint uses
    // infos
    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = ""; // clean up this from time to time!

    public string clientMsg = "";//write a message here and press enter while client is running to send a messag to server

    public bool isServer = false;//click on this to run a server.
    public bool isClient = false;//click on this to run a client.
    public bool isRunning = false;//indicator light
    public bool startRunning = false;//starts server/client depending on which one is marked true.
    public bool kill = false;//kill thread.


    public void FixedUpdate()
    {
        if (startRunning && !isRunning)
        {
            startRunning = false;
            if (isServer)
            {
                StartServer();
            }
            if (isClient)
            {
                StartClient();
            }
        }

        if (kill && Thread.IsAlive)
        {
            kill = false;
            isRunning = false;
            Debug.Log("thread killed" + Thread.ThreadState);
            
        }
    }

    //the comments are in german because i stole half of this shit from a stackoverflow.
    //i have no idea what half of the shit means and im too lazy to delete it all.


    public void StartClient()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("starting UDP client");
        

        // ----------------------------
        // Senden
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        client.Client.ReceiveTimeout = 5000;
        // status
        print("Sending to " + IP + " : " + port);
        print("Testing: nc -lu " + IP + " : " + port);

        //make threaad here n start it.
        Thread = new Thread(new ThreadStart(SendString));
        Thread.IsBackground = true;
        Thread.Start();
        isRunning = true;
        Debug.Log("client started");
    }
    
    // sendData
    private void SendString()
    {
        do
        {
            try
            {
                if (clientMsg != "")
                {
                    byte[] data = Encoding.UTF8.GetBytes(clientMsg);

                    client.Send(data, data.Length, remoteEndPoint);
                    clientMsg = "";
                }
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        } while (isRunning);
    }

    // this initiates a server.
    private void StartServer()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("starting server");
        
        client = new UdpClient(port);
        client.Client.ReceiveTimeout = 5000;

        // status
        print("Sending to 127.0.0.1 : " + port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


        //make threaad here n start it.
        Thread = new Thread(new ThreadStart(ReceiveData));
        Thread.IsBackground = true;
        Thread.Start();
        isRunning = true;
        Debug.Log("server started");
    }

    // receive thread
    private void ReceiveData()
    {
        do
        {
            try
            {
                // Bytes empfangen.
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                string text = Encoding.UTF8.GetString(data);

                // Den abgerufenen Text anzeigen.
                print(">> " + text);

                // latest UDPpacket
                lastReceivedUDPPacket = text;

                // ....
                allReceivedUDPPackets = allReceivedUDPPackets + text;

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        } while (isRunning);
    }
}