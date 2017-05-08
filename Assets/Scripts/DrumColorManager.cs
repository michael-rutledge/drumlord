using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumColorManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // set color of drums based on type
        switch (this.name)
        {
            case "SnareDrum":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.snareColor;
                break;
            case "HiHatImposter":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.hiHatColor;
                break;
            case "CrashImposter":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.crashColor;
                break;
            case "RideImposter":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.rideColor;
                break;
            case "HighTom":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.highTomColor;
                break;
            case "MedTom":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.medTomColor;
                break;
            case "LowTom":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.lowTomColor;
                break;
            case "BassDrum":
                this.GetComponent<MeshRenderer>().material.color = ApplicationModel.bassColor;
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
