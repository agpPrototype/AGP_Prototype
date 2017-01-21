using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using Utility;

public class UserInput : MonoBehaviour {

    private InputDevice m_device = InputManager.ActiveDevice;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        GetInputs();
        
    }

    void GetInputs()
    {

    }
}
