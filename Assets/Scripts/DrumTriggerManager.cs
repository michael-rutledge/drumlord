using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumTriggerManager : MonoBehaviour {

    // SteamVR data
    public SteamVR_TrackedObject rightHand;
    private SteamVR_Controller.Device rightDevice;
    public SteamVR_TrackedObject leftHand;
    private SteamVR_Controller.Device leftDevice;
    // Time data
    public double rightHit;
    public double leftHit;
    public double startTime;
    private const float AUDIO_DELAY = 0.1f;
    // Position data
    public GameObject stickRightHead;
    public GameObject stickLeftHead;
    private Vector3 oldRightPos;
    private Vector3 oldLeftPos;

    // Use this for initialization
    void Start () {
        rightHit = leftHit = -1.0;
        startTime = Time.fixedTime + AUDIO_DELAY;
        oldRightPos = stickRightHead.transform.position;
        oldLeftPos = stickLeftHead.transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        rightDevice = SteamVR_Controller.Input((int)rightHand.index);
        leftDevice = SteamVR_Controller.Input((int)leftHand.index);
        oldRightPos = stickRightHead.transform.position;
        oldLeftPos = stickLeftHead.transform.position;
    }

    // To be called when other objects collide
    void OnTriggerEnter(Collider other)
    {
        float curTime = Time.fixedTime;
        // Check if right stick hits drum
        if (other.gameObject.name.Equals("StickRightHead") &&
            oldRightPos.y > stickRightHead.transform.position.y)
        {
            SteamVR_Controller.Input((int)rightHand.index).TriggerHapticPulse((ushort)3999);
            float angM = rightDevice.angularVelocity.magnitude;
            rightHit = curTime - startTime;
            Debug.Log(this.name + " collision with " + other.gameObject.name +
                " with velocity " + angM + " at time " + rightHit);
        }
        // Check if left stick hits drum
        if (other.gameObject.name.Equals("StickLeftHead") &&
            oldLeftPos.y > stickLeftHead.transform.position.y)
        {
            SteamVR_Controller.Input((int)leftHand.index).TriggerHapticPulse((ushort)3999);
            float angM = leftDevice.angularVelocity.magnitude;
            leftHit = curTime - startTime;
            Debug.Log(this.name + " collision with " + other.gameObject.name +
                " with velocity " + angM + " at time " + leftHit);
        }
    }

    // To be called when colliding objects leave contact
    void OnTriggerExit(Collider other)
    {

    }

    void OnGUI() {
        GUI.Label( new Rect(450, 100, 100, 100),
            ("" + rightHit) );
    }
}
