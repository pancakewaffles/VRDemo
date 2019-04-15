using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretThumbController : MonoBehaviour {
    // Use this for initialization
    private Vector3 initialpos;
    private Vector3 velocity = Vector3.zero;

    private 
    void Start () {
        initialpos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        //read controller input: N.B. unity inputs must be set up with these exact names (Project Settings/Input)
        float Xin = Input.GetAxis("MoveVertical");
        float Yin = Input.GetAxis("MoveHorizontal");
        Vector3 minput = new Vector3(Xin, Yin, 0.0f);
        //move this object
        transform.position = Vector3.SmoothDamp(transform.position, initialpos + minput*2, ref velocity, 1.0f);
    }

}
