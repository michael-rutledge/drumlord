﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumTriggerManager : MonoBehaviour {

    // SteamVR data
    public SteamVR_TrackedObject rightHand;
    private SteamVR_Controller.Device rightDevice;
    public SteamVR_TrackedObject leftHand;
    private SteamVR_Controller.Device leftDevice;
    // Time data
    public SongManager songManager;
    public double rightHit;
    public double leftHit;
    // Position data
    public GameObject stickRightHead;
    public GameObject stickLeftHead;
    private Vector3 oldRightPos;
    private Vector3 oldLeftPos;

    // Use this for initialization
    void Start () {
        // stick information initialized
        rightHit = leftHit = -1.0;
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
            float tranM = rightDevice.velocity.magnitude;
            float velM = angM + tranM;
            rightHit = songManager.curTime;
            //Debug.Log(this.name + " collision with " + other.gameObject.name +
            //    " with velocity " + velM + " at time " + rightHit);
        }
        // Check if left stick hits drum
        if (other.gameObject.name.Equals("StickLeftHead") &&
            oldLeftPos.y > stickLeftHead.transform.position.y)
        {
            SteamVR_Controller.Input((int)leftHand.index).TriggerHapticPulse((ushort)3999);
            float angM = leftDevice.angularVelocity.magnitude;
            float tranM = leftDevice.velocity.magnitude;
            float velM = angM + tranM;
            leftHit = songManager.curTime;
            //Debug.Log(this.name + " collision with " + other.gameObject.name +
            //    " with velocity " + velM + " at time " + leftHit);
        }
    }

    // To be called when colliding objects leave contact
    void OnTriggerExit(Collider other)
    {

    }
    
}
