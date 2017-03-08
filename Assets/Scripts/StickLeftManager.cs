using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickLeftManager : MonoBehaviour {

    private SteamVR_Controller.Device device;
    public StickRightManager rightStick;
    public SteamVR_TrackedObject leftHand;
    private SteamVR_Controller.Device leftDevice;
    public bool actionMenuPressed;

    // Use this for initialization
    void Start () {
        actionMenuPressed = false;
	}
	
	// Update is called once per frame
	void Update () {
        leftDevice = SteamVR_Controller.Input((int)leftHand.index);
        if (leftDevice.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            actionMenuPressed = true;
            SteamVR.instance.hmd.ResetSeatedZeroPose();
            Debug.Log("LEFT BUTTON PRESS");
        }
        else if (leftDevice.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            actionMenuPressed = false;
            Debug.Log("LEFT BUTTON RELEASE");
        }
    }
}
