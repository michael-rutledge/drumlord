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
                this.GetComponent<MeshRenderer>().material.color = new Color(1, .75f, .75f, 1);
                break;
            case "HiHatImposter":
                this.GetComponent<MeshRenderer>().material.color = new Color(.99f, .99f, .59f, 1);
                break;
            case "CrashImposter":
                this.GetComponent<MeshRenderer>().material.color = new Color(.47f, .87f, .47f, 1);
                break;
            case "RideImposter":
                this.GetComponent<MeshRenderer>().material.color = new Color(.63f, .77f, 1, 1);
                break;
            case "HighTom":
                this.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 1.0f, 1);
                break;
            case "MedTom":
                this.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 1.0f, 1);
                break;
            case "LowTom":
                this.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 1.0f, 1);
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
