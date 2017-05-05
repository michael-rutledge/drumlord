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
    private string midiDataDir;
    // button game object stuff
    private float buttonHeight = 0;
    private List<GameObject> songButtons = new List<GameObject>();
    public GameObject playButton;
    // track description stuff
    private GameObject albumArt = null, songNameText = null, songDescription = null;
    private string curSongName = null;


	// Use this for initialization
	void Start () {
        // get string arrays for the song data
        midiDataDir = Application.streamingAssetsPath;
        songDataIds = System.IO.Directory.GetFiles(midiDataDir, "*.mid");
        for (int i = 0; i < songDataIds.Length; i++)
        {
            // take the absolute path off of the ids
            songDataIds[i] = System.IO.Path.GetFileName(songDataIds[i]);
            songDataIds[i] = songDataIds[i].Substring(0, songDataIds[i].Length - 4);
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
            // check for info file and change the title to grammatically correct form
            TextAsset infoFile = (TextAsset)Resources.Load("SongData/" + button.name + "/"
                + button.name + "Info");
            string[] lines = null;
            if (infoFile != null)
            {
                lines = infoFile.text.Split('\n');
                button.GetComponentInChildren<Text>().text = lines[0];
            }
            // give it the actions
            button.GetComponent<Button>().onClick.AddListener(() => showMetaData(curId));
            // add it to the list
            songButtons.Add(button);
            Debug.Log(curId);
        }
	}
	

	// Update is called once per frame
	void Update () {
		if (ApplicationModel.selectedSongId == null)
        {
            playButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            playButton.GetComponent<Button>().interactable = true;
        }
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
        if (Resources.Load<Sprite>("SongData/" + name + "/"  + name + "Art") != null)
        {
            albumArt.GetComponent<Image>().sprite =
                Resources.Load<Sprite>("SongData/" + name + "/" + name + "Art");
        }
        // check for info file
        TextAsset infoFile = (TextAsset)Resources.Load("SongData/" + name + "/" + name + "Info");
        string[] lines = null;
        if (infoFile != null)
        {
            lines = infoFile.text.Split('\n');
        }
        // create songNameText and destroy if already there
        if (songNameText != null)
            DestroyImmediate(songNameText);
        songNameText = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "SongNameText"));
        songNameText.transform.SetParent(GameObject.Find("SongMenuCanvas").transform, false);
        // edit songNameText if info file is given
        if (lines != null)
        {
            songNameText.GetComponent<Text>().text = lines[0];
            curSongName = lines[0];
        }
        // create songDescription and destroy if already there
        if (songDescription != null)
            DestroyImmediate(songDescription);
        songDescription = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "SongDescription"));
        songDescription.transform.SetParent(GameObject.Find("SongMenuCanvas").transform, false);
        // edit songDescription
        string description = "";
        for (int i = 0; lines != null &&  i < lines.Length; i++)
        {
            description += lines[i] + "\n";
        }
        songDescription.GetComponent<Text>().text = description;
    }


    // play button function
    public void playSong()
    {
        // get difficulty from dropdown
        GameObject diffMenu = GameObject.Find("Dropdown");
        int diffIndex = diffMenu.GetComponent<Dropdown>().value;
        Dropdown.OptionData optData = diffMenu.GetComponent<Dropdown>().options[diffIndex];
        ApplicationModel.difficulty = optData.text;
        ApplicationModel.songName = curSongName;
        Debug.Log("PLaying Level at difficulty: " + ApplicationModel.difficulty);
        // load play scene
        if (ApplicationModel.selectedSongId != null)
            SceneManager.LoadScene("playScene");
    }
}
