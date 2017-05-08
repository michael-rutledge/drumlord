using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultScreenManager : MonoBehaviour {

    public GameObject retryButton, startMenuButton;

	// Use this for initialization
	void Start () {
        // change data in text to reflect previous performance
        GameObject.Find("SongNameText").GetComponent<Text>().text = ApplicationModel.songName;
        GameObject.Find("DifficultyText").GetComponent<Text>().text =
            "Difficulty: " + ApplicationModel.difficulty;
        GameObject.Find("PercentHitText").GetComponent<Text>().text =
            "Percent Notes Hit: " + ApplicationModel.percentHit + "%";
        GameObject.Find("NotesMissedText").GetComponent<Text>().text =
            "Notes Missed: " + ApplicationModel.notesMissed;
        GameObject.Find("HighStreakText").GetComponent<Text>().text =
            "Longest Streak: " + ApplicationModel.highStreak + " notes";
        GameObject.Find("ScoreText").GetComponent<Text>().text =
            "Final Score: " + ApplicationModel.score;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // retry song
    public void retrySong()
    {
        ApplicationModel.score = 0;
        ApplicationModel.highStreak = 0;
        ApplicationModel.notesMissed = 0;
        ApplicationModel.percentHit = 0;
        SceneManager.LoadScene("playScene");
    }

    // go to main menu
    public void gotoStartMenu()
    {
        ApplicationModel.score = 0;
        ApplicationModel.highStreak = 0;
        ApplicationModel.notesMissed = 0;
        ApplicationModel.percentHit = 0;
        ApplicationModel.selectedSongId = null;
        ApplicationModel.songName = null;
        SceneManager.LoadScene("startMenu");
    }
}
