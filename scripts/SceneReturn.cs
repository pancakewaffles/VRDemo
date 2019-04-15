using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReturn : MonoBehaviour
{
    public void Return()
    {
        UnityEngine.Debug.Log("Scene return called");
        if(SceneManager.GetActiveScene().name == "CarInterior")
        {
            Destroy(GameObject.Find("BackScreen"));
        }
        Destroy(GameObject.Find("Screen"));
        Destroy(GameObject.Find("ControllerReader"));
        if (SceneManager.GetActiveScene().name == "VRScene")
        {
            Destroy(GameObject.Find("Wall3"));
        }
        SceneManager.LoadScene("SceneSelect"); 
    }  
    
}
