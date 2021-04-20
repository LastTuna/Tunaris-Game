using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TGNetworkMan : MonoBehaviour {

	// Use this for initialization
	void Start () {

        UdpClient udpClient = new UdpClient(11000);
        try
        {
            udpClient.Connect("localhost", 11000);

            // Sends a message to the host to which you have connected.
            Byte[] sendBytes = Encoding.ASCII.GetBytes("i cant sneed");

            udpClient.Send(sendBytes, sendBytes.Length);
            Debug.Log("package sent ");
            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Debug.Log("listening");

            // Blocks until a message returns on this socket from a remote host.
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            

            // Uses the IPEndPoint object to determine which of these two hosts responded.
            Debug.Log("This is the message you received " +
                                         returnData.ToString());
            Debug.Log("This message was sent from " +
                                        RemoteIpEndPoint.Address.ToString() +
                                        " on their port number " +
                                        RemoteIpEndPoint.Port.ToString());

            udpClient.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
	
}
