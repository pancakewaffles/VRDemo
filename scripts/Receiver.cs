using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Collections.Generic;

public class Receiver : MonoBehaviour
{
    // Network parameters
    public int port = 3142;
    public string IP = "192.168.43.46";
    TcpClient client;

    // Texture of screen
    Texture2D tex;

    // Stopping parameters
    private bool stop = false;
    private bool connectionEstablished = false;
    
    // Use this for initialization
    void Start()
    {
        stop = false;
        tex = new Texture2D(0, 0);
        client = new TcpClient();

        //Connect to server from another Thread
        Loom.RunAsync(() =>
        { // put a while(true) if(!connectionEstablished) ...
                try
                {
                EstablishConnection();
                imageReceiver();
                
                }catch(Exception en)
                {
                    Debug.Log("[Receiver.cs] Socket Connection disconnected; retry establish " + en.ToString());
                    connectionEstablished = false;
                }
        });
    }

    private void EstablishConnection()
    {
        Debug.Log("[Receiver.cs] Connecting to server...");
        client.Connect(IPAddress.Parse(IP), port);
        connectionEstablished = true;
        Debug.Log("[Receiver.cs] Connected!");
    }

    void imageReceiver()
    {
        //While loop in another Thread is fine so we don't block main Unity Thread
        Loom.RunAsync(() =>
        {
            while (!stop)
            {
                //Read Image Count
                //int imageSize = readImageByteSize(SEND_RECEIVE_COUNT);
                //Debug.Log("Received Image byte Length: " + imageSize);

                //Read Image Bytes and Display it
                readFrameByteArray(73728);
            }
        });
    }


    //Converts the data size to byte array and put result to the fullBytes array
    void byteLengthToFrameByteArray(int byteLength, byte[] fullBytes)
    {
        //Clear old data
        Array.Clear(fullBytes, 0, fullBytes.Length);
        //Convert int to bytes
        byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
        //Copy result to fullBytes
        bytesToSendCount.CopyTo(fullBytes, 0);
    }

    //Converts the byte array to the data size and returns the result
    int frameByteArrayToByteLength(byte[] frameBytesLength)
    {
        int byteLength = BitConverter.ToInt32(frameBytesLength, 0);
        return byteLength;
    }


    /////////////////////////////////////////////////////Read Image Data Byte Array from Server///////////////////////////////////////////////////
    private void readFrameByteArray(int size)
    {
        bool disconnected = false;
        size = 2;
        NetworkStream serverStream = client.GetStream();
        List<byte> arr = new List<byte>();
        byte[] readerBytes = new byte[size];
        var total = 0;
        do
        {
            var read = serverStream.Read(readerBytes, total, size-total);
            if (read == 0)
            {
                disconnected = true;
                break;
            }
            total += read;
            arr.Add(readerBytes[0]);
            arr.Add(readerBytes[1]);
            //Debug.LogFormat("Client recieved {0} bytes", total);
            if (readerBytes[0] == 0xff && readerBytes[1] == 0xd9)
            {
                //Debug.Log("[Receiver.cs] Recognised end of image");
                break;

            }
            total = 0;
        } while (true);

        byte[] imageBytes = arr.ToArray();

        bool readyToReadAgain = false;
        //Display Image
        if (!disconnected)
        {
            //Array.Reverse(imageBytes);
            //File.WriteAllBytes("img.jpg", imageBytes);
            //Display Image on the main Thread
            Loom.QueueOnMainThread(() =>
            {
                displayReceivedImage(imageBytes);
                readyToReadAgain = true;
                
            });
        }
        //Wait until old Image is displayed
        while (!readyToReadAgain)
        {
            System.Threading.Thread.Sleep(1);
        }
    }


    void displayReceivedImage(byte[] receivedImageBytes)
    {
        //Debug.Log("[Receiver.cs] Displaying Received Image");
        //Debug.Log(receivedImageBytes.Length);
        tex.LoadImage(receivedImageBytes);
        var renderer = GetComponent<Renderer>();
	    renderer.material.mainTexture = tex;
    }

    
    // Update is called once per frame
    void Update()
    {


    }
    void OnDestroy()
    {
        Debug.Log("[Receiver.cs] OnApplicationQuit");
        stop = true;
        if (client != null)
        {
            client.Close();
        }
        

    }


    void OnApplicationQuit()
    {
        Debug.Log("[Receiver.cs] OnApplicationQuit");
        stop = true;
        if(client != null)
        {
            client.Close();
        }
        
    }
}
