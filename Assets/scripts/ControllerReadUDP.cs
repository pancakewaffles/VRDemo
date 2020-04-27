using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ControllerReadUDP : MonoBehaviour
{
    /* Overview:
     * reads the joystick input, sends this over UDP to server
     * reads the car feedback data into "RecievedCarData" int
    */

    //public strings parsed as IP and port (to allow external setting of IP addr)
    public string IP = "192.168.43.46";
    public string sendport = "3002";
    public string recieveport = "3003";

    //Maximum boost health
    public int MaxBoostHealth = 1000;
    //BostRegenRate
    public int BoostRegenRate = 2;
    //boost depletion rate:
    public int BoostDepletionRate = 12;
    //current boost health
    private int BoostHealth;
    //combined trigger values
    private int TrigCombined;
    //Boost value sent over
    private int SendBoost;
    //Slider object
    public Slider BoostSlider;

    //first 8 bits = X-axis, next 8 bits = Y-axis, next 8 bits = boost amount
    private int combinedXY;
    //Recieved Data
    public int RecievedCarData;

    //Send rate
    public readonly int SSleepTime = 100;
    //Recieve rate
    public readonly int RSleepTime = 100;
    //false on app quit to terminate threads
    private bool runThreads = true;
    //UDP informtaion:
    UdpClient sendclient;
    UdpClient recieveclient;
    IPEndPoint SpiEndPoint;
    IPEndPoint RpiEndPoint;
    
    void Start()
    {
        BoostHealth = MaxBoostHealth;
        SendBoost = 0;
        BoostSlider.maxValue = MaxBoostHealth;
        BoostSlider.minValue = 0;

        SpiEndPoint = new IPEndPoint(IPAddress.Parse(IP), Int32.Parse(sendport));
        RpiEndPoint = new IPEndPoint(IPAddress.Parse(IP), Int32.Parse(recieveport));
        sendclient = new UdpClient(Int32.Parse(sendport));
        recieveclient = new UdpClient(Int32.Parse(recieveport));
        
        //Start Sending and Recieving threads
        SendtoCar();
        RecievefromCar();
    }


    void Update()
    {
        //read controller input: N.B. unity inputs must be set up with these exact names instructions in JoystickSetup.txt)
        float Xin = Input.GetAxis("MoveVertical");
        float Yin = Input.GetAxis("MoveHorizontal");

        //Read controller triggers and combine into a single boost amount
        float LTrig = Input.GetAxis("LeftTrigger");
        float RTrig = Input.GetAxis("RightTrigger");
        TrigCombined = (int)(BoostDepletionRate * (LTrig + RTrig));

        //HORN :)
        int horn = Input.GetKey(KeyCode.Joystick1Button0) ? 1 : 0;
        int plag = (Input.GetKey(KeyCode.Joystick1Button1) && Input.GetKey(KeyCode.Joystick1Button5)) ? 2 : 0;

        //update combinedXY
        int sendX = (int)((Xin + 1) * 127.5f);
        int sendY = (int)((Yin + 1) * 127.5f);
        combinedXY = sendX + (sendY * 256) + (SendBoost * 65536) + ((horn + plag) * 16777216);
    }


    //An Update independent of frame rate: means depletion/regen of boost bar is independent of fps
    void FixedUpdate()
    {
        SendBoost = 0;
        //adjust the boost health
        if (BoostHealth > BoostDepletionRate)
        {
            BoostHealth = Math.Max(0, BoostHealth - TrigCombined);
            SendBoost = Math.Min(255, TrigCombined * (128 / BoostDepletionRate));
        }
        if (TrigCombined == 0) BoostHealth = Math.Min(MaxBoostHealth, (BoostHealth + BoostRegenRate));
        BoostSlider.value = BoostHealth;
    }

    private void RecievefromCar()
    {
        Loom.RunAsync(() =>
        {
            while (runThreads)
            {
                //Recieve Data from Car:
                try
                {
                    byte[] recieveddata = recieveclient.Receive(ref RpiEndPoint);
                    RecievedCarData = BitConverter.ToInt32(recieveddata, 0);
                    Debug.Log("[CRUDP] recieved " + RecievedCarData);
                }
                catch (Exception err)
                {
                    Debug.Log("[CRUDP] error on data recieve" + err);
                }
                System.Threading.Thread.Sleep(RSleepTime);
            }
        });
    }

    private void SendtoCar()
    {
        Loom.RunAsync(() =>
        {
            while (runThreads)
            {
                //Send Data to Car:
                try
                {
                    byte[] datatosend = BitConverter.GetBytes(combinedXY);
                    sendclient.Send(datatosend, datatosend.Length, SpiEndPoint);
                    //Debug.Log("[CRUDP] sent " + datatosend.Length + " bytes successfully");
                }
                catch (Exception err)
                {
                    Debug.Log("[CRUDP] error on data send" + err);
                }
                System.Threading.Thread.Sleep(SSleepTime);
            }
        });
    }
    void OnDestroy()
    {
        if (sendclient != null)
        {
            sendclient.Close();
        }
        if (recieveclient != null)
        {
            recieveclient.Close();
        }
        runThreads = false;

    }


    void OnApplicationQuit()
    {
        if(sendclient != null)
        {
            sendclient.Close();
        }
        if(recieveclient != null)
        {
            recieveclient.Close();
        }
        runThreads = false;
        
    }
}