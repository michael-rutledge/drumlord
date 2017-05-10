using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationModel : MonoBehaviour {

    // bass available from main menu
    public static bool bassAvailable = false;
    public static KeyCode bassKey;
    // song metadata
    public static string selectedSongId = null;
    public static string songName = null;
    public static string difficulty = "Expert";
    // result data
    public static int notesMissed;
    public static int percentHit;
    public static int score;
    public static int highStreak;
    // drum colors
    public static Color hiHatColor = new Color(1, 1, 0, 1);
    public static Color snareColor = new Color(1, 0, 0, 1);
    public static Color crashColor = new Color(0, 1, 0, 1);
    public static Color rideColor = new Color(0.67f, 0, 1, 1);
    public static Color highTomColor = new Color(0, 1, 1, 1);
    public static Color medTomColor = new Color(0.0f, 0.5f, 1.0f, 1);
    public static Color lowTomColor = new Color(1.0f, 0.5f, 1.0f, 1);
    public static Color bassColor = new Color(1.0f, 0.67f, 0.0f, 1);

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
