using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            // end playScene if right controller menu button is hit
            if (SceneManager.GetActiveScene().name == "playScene")
            {
                ApplicationModel.score = 0;
                ApplicationModel.highStreak = 0;
                ApplicationModel.notesMissed = 0;
                ApplicationModel.percentHit = 0;
                ApplicationModel.selectedSongId = null;
                ApplicationModel.songName = null;
                SceneManager.LoadScene("startMenu");
            }
            actionMenuPressed = true;
        }
        else if (rightDevice.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            actionMenuPressed = false;
        }
    }

}
