using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
           // Start in fresh reset position
        SteamVR.instance.hmd.ResetSeatedZeroPose();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
