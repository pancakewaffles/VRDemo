using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbController : MonoBehaviour {
    private Vector3 initialpos;
    private Vector3 initialpos_screenspace;
    private GameObject JoyStick;
    private Camera cam;
    private RectTransform rectTransform;
    // Use this for initialization
    void Start () {
        Time.timeScale = 1;
        JoyStick = GameObject.Find("Joystick");
        initialpos = JoyStick.transform.position;
        transform.position = initialpos;
        rectTransform = GetComponent<RectTransform>();
        cam = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update () {
        //read controller input: N.B. unity inputs must be set up with these exact names (Project Settings/Input)
        float Xin = Input.GetAxis("MoveVertical");
        float Yin = Input.GetAxis("MoveHorizontal");
        Vector3 minput = new Vector3(Xin, Yin, 0.0f);
        //move this object
        initialpos = JoyStick.transform.position;
        initialpos_screenspace = cam.WorldToViewportPoint(initialpos);
        //Debug.Log(initialpos_screenspace);
        transform.position = cam.ViewportToWorldPoint(initialpos_screenspace + minput * 0.02f);
    }

}
