using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SongMenuManager : MonoBehaviour {

    // constants
    private const string PREFAB_DIR = "Prefabs/";
    private const float BUTTON_OFFSET = 40;
    // meta data stuff
    private string[] songDataIds;
    private string songDataDir = "Assets/Resources/SongData/";
    // button game object stuff
    private float buttonHeight = 0;
    private List<GameObject> songButtons = new List<GameObject>();
    // track description stuff
    private GameObject albumArt = null, songNameText = null, songDescription = null;

	// Use this for initialization
	void Start () {
        // get string arrays for the song data
        songDataIds = System.IO.Directory.GetDirectories(songDataDir);
        for (int i = 0; i < songDataIds.Length; i++)
        {
            // take the absolute path off of the ids
            songDataIds[i] = songDataIds[i].Substring(songDataDir.Length);
            string curId = songDataIds[i];
            // instantiate the button and put in the scroll view
            GameObject button = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "ScrollViewButton"));
            button.transform.SetParent(GameObject.Find("SongButtons").transform, false);
            Vector3 oldPos = button.transform.localPosition;
            button.transform.localPosition = new Vector3(oldPos.x, oldPos.y - buttonHeight, oldPos.z);
            // edit the button's text and name
            buttonHeight += BUTTON_OFFSET;
            button.name = curId;
            button.GetComponentInChildren<Text>().text = button.name;
            // give it the actions
            button.GetComponent<Button>().onClick.AddListener(() => showMetaData(curId));
            // add it to the list
            songButtons.Add(button);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void showMetaData(string name)
    {
        // set selected song for play
        ApplicationModel.selectedSongId = name;
        // create albumArt and destroy if already there
        if (albumArt != null)
            DestroyImmediate(albumArt);
        albumArt = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "AlbumArt"));
        albumArt.transform.SetParent(GameObject.Find("SongMenuCanvas").transform, false);
        // use custom art if given within songData
        if (Resources.Load(songDataDir + name + "Art") != null)
        {
            albumArt.GetComponent<Image>().sprite = (Sprite)Resources.Load(songDataDir + name + "Art");
        }
        // check for info file
        System.IO.StreamReader infoFile = null;
        try
        {
            infoFile =
                new System.IO.StreamReader(songDataDir + name + "/" + name + "Info.txt");
        }
        catch
        {
            Debug.Log("File not found");
            return;
        }
        // create songNameText and destroy if already there
        if (songNameText != null)
            DestroyImmediate(songNameText);
        songNameText = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "SongNameText"));
        songNameText.transform.SetParent(GameObject.Find("SongMenuCanvas").transform, false);
        // edit songNameText if info file is given
        songNameText.GetComponent<Text>().text = infoFile.ReadLine();
        // create songDescription and destroy if already there
        if (songDescription != null)
            DestroyImmediate(songDescription);
        songDescription = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "SongDescription"));
        songDescription.transform.SetParent(GameObject.Find("SongMenuCanvas").transform, false);
        // edit songDescription
        string description = "";
        string line;
        while ((line = infoFile.ReadLine()) != null)
        {
            description += line + "\n";
        }
        songDescription.GetComponent<Text>().text = description;
    }


    // play button function
    public void playSong()
    {
        if (ApplicationModel.selectedSongId != null)
            SceneManager.LoadScene("basicMotionTest");
    }
}
