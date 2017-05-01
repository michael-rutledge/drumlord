using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongMenuManager : MonoBehaviour {

    // constants
    private const string PREFAB_DIR = "Prefabs/";
    private const float BUTTON_OFFSET = 40;
    // meta data stuff
    private string[] songDataFiles;
    private string[] songDataIds;
    private string songDataDir = "Assets/Resources/SongData";
    // button game object stuff
    private float buttonHeight = 0;
    private List<GameObject> songButtons = new List<GameObject>();

	// Use this for initialization
	void Start () {
        // get string arrays for the song data
        songDataFiles = System.IO.Directory.GetDirectories(songDataDir);
        songDataIds = System.IO.Directory.GetDirectories(songDataDir);
        for (int i = 0; i < songDataIds.Length; i++)
        {
            // take the absolute path off of the ids
            songDataIds[i] = songDataIds[i].Substring(songDataDir.Length+1);
            // make the button and put in the scroll view
            GameObject button = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "ScrollViewButton"));
            button.transform.SetParent(GameObject.Find("SongButtons").transform, false);
            Vector3 oldPos = button.transform.localPosition;
            button.transform.localPosition = new Vector3(oldPos.x, oldPos.y - buttonHeight, oldPos.z);
            // edit the button's text and name
            buttonHeight += BUTTON_OFFSET;
            button.name = songDataIds[i];
            button.GetComponentInChildren<Text>().text = button.name;
            // give it the actions
            // add it to the list
            songButtons.Add(button);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void fuckItUp()
    {
        string yeah = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
        Debug.Log(yeah);
    }
}
