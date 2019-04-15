using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneChanger : MonoBehaviour {
    public void Left()
    {
        SceneManager.LoadScene("SpaceBase");
    }
    public void Center()
    {
        SceneManager.LoadScene("OutdoorScene");
    }
    public void Right()
    {
        SceneManager.LoadScene("CarInterior");
    }
    public void SecretBehind()
    {
        SceneManager.LoadScene("VRScene");
    }

}
