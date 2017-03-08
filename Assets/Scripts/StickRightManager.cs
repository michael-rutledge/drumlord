using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickRightManager : MonoBehaviour
{

    private SteamVR_Controller.Device device;
    public StickLeftManager leftStick;
    public SteamVR_TrackedObject rightHand;
    private SteamVR_Controller.Device rightDevice;
    public bool actionMenuPressed;

    // Use this for initialization
    void Start()
    {
        actionMenuPressed = false;
    }

    // Update is called once per frame
    void Update()
    {
        rightDevice = SteamVR_Controller.Input((int)rightHand.index);
        if (rightDevice.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            //SteamVR.instance.hmd.ResetSeatedZeroPose();
            actionMenuPressed = true;
            Debug.Log("RIGHT BUTTON PRESS");
        }
        else if (rightDevice.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            actionMenuPressed = false;
            Debug.Log("RIGHT BUTTON RELEASE");
        }
    }

}
