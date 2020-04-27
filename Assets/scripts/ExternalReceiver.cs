using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;

public class ExternalReceiver: MonoBehaviour
{
    //path to the GitHub project folder
    public string projectPath = "D:\\Users\\Chavez Tan\\Documents\\GitHub\\Eyes-on-the-Road\\";
    public string cameraType = "front";
    private System.Diagnostics.Process decoderProcess;

    public int port_front = 3142;
    public int port_back = 3143;
    public string IP = "127.0.0.1";
    TcpClient client;
    NetworkStream networkStream;

    Texture2D imageTexture;
    const int WIDTH = 640;
    const int HEIGHT = 480;

    private bool stop = false;

    //using globals to save memory
    //raw data is 4 bytes per pixel - RGBA
    private byte[] dropFrameBuffer = new byte[WIDTH * HEIGHT * 4];
    private byte[] textureBuffer = new byte[WIDTH * HEIGHT * 4];

    void Start()
    {
        StartDecodeService();
        client = new TcpClient();
        imageTexture = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGBA32, false, true);

        try {
            EstablishConnection();
            imageReceiver();
        } catch(Exception en) {
            Debug.Log("Socket Connection disconnected; retry establish " + en.ToString());
        }
    }

    private void StartDecodeService() {
        System.Diagnostics.ProcessStartInfo procInfo = new System.Diagnostics.ProcessStartInfo();
        procInfo.FileName = "cmd.exe";
        procInfo.WorkingDirectory = projectPath;
        procInfo.Arguments = "/C node avc-vr\\decode.js " + cameraType;
        procInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
        procInfo.CreateNoWindow = true;
        procInfo.UseShellExecute = false;

        decoderProcess = new System.Diagnostics.Process();
        decoderProcess.StartInfo = procInfo;
        decoderProcess.Start();
    }

    private void EstablishConnection()
    {
        Debug.Log("Connecting to socket...");
        client.Connect(IPAddress.Parse(IP), cameraType == "front" ? port_front : port_back);
        networkStream = client.GetStream();
        Debug.Log("Connected!");
    }

    void imageReceiver()
    {
        //While loop in another Thread is fine so we don't block main Unity Thread
        Loom.RunAsync(() => {
            while (!stop) {
                readFrameByteArray();
            }
        });
    }

    private void decodeImage(byte[] buffer) {
        imageTexture.LoadRawTextureData(buffer);
        imageTexture.Apply(false);

        var renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = imageTexture;
    }

    private static System.Diagnostics.Stopwatch startTime() {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        return stopWatch;
    }

    private static void endTime(System.Diagnostics.Stopwatch stopWatch) {
        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        TimeSpan ts = stopWatch.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
        ts.Hours, ts.Minutes, ts.Seconds,
        ts.Milliseconds);
        Debug.Log("RunTime " + elapsedTime);
    }

    private void readBytes(byte[] byteBuffer) {
        int size = byteBuffer.Length;
        int bytesRead = 0;
        while (bytesRead < size) {
            int read = networkStream.Read(byteBuffer, bytesRead, size - bytesRead);
            if (read <= 0) {
                // Debug.Log("Empty read");
                break;
            }
            bytesRead += read;
        }
    }

    private void dropFrame() {
        readBytes(dropFrameBuffer);
    }

    private void readFrameByteArray()
    {
        readBytes(textureBuffer);

        bool readyToReadAgain = false;

        //Display Image on the main Thread
        Loom.QueueOnMainThread(() => {
            decodeImage(textureBuffer);
            readyToReadAgain = true;
        });

        //Wait until old Image is displayed
        while (!readyToReadAgain) {
            dropFrame();
        }
    }
    void OnDestroy()
    {
        stop = true;

        if (client != null) {
            client.Close();
        }
        if (decoderProcess != null) {
            decoderProcess.Kill();
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        stop = true;

        if (client != null) {
            client.Close();
        }
        if (decoderProcess != null) {
            decoderProcess.Kill();
        }
    }
}
