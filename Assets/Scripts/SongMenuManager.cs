using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SongMenuManager : MonoBehaviour {

    // constants
    private const string PREFAB_DIR = "Prefabs/";
    private const float BUTTON_OFFSET = 40;
    private const string fPrefix = "file://";
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
        songDataIds = System.IO.Directory.GetDirectories(Application.streamingAssetsPath);
        for (int i = 0; i < songDataIds.Length; i++)
        {
            // take the absolute path off of the ids
            string curId = System.IO.Path.GetFileName(songDataIds[i]);
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
            System.IO.StreamReader infoFile = 
                new System.IO.StreamReader(songDataIds[i] + "/" + curId + "Info.txt");
            string line = null;
            if ((line = infoFile.ReadLine()) != null)
            {
                button.GetComponentInChildren<Text>().text = line;
            }
            // give it the actions
            button.GetComponent<Button>().onClick.AddListener(() => showMetaData(curId));
            // add it to the list
            songButtons.Add(button);
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
        // check for bass pedal input
        if (Input.inputString != "")
        {
            foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                // call if key is down
                if (Input.GetKeyDown(kcode))
                {
                    ApplicationModel.bassAvailable = true;
                    ApplicationModel.bassKey = kcode;
                    GameObject.Find("BassText").GetComponent<Text>().text = "Bass pedal found! Bound to key: \"" +
                        kcode.ToString() + "\"";
                }
            }
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
        // use custom art if given within songData, check for different extensions
        WWW wArt = fetchImageWWW(fPrefix + Application.streamingAssetsPath + "/" + name + "/" +
            name + "Art");
        if (wArt != null)
        {
            // create sprite from loaded image
            Texture artTex = wArt.texture;
            Sprite sprite = Sprite.Create(artTex as Texture2D,
                new Rect(0, 0, artTex.width, artTex.height), Vector2.zero);
            // use said sprite
            albumArt.GetComponent<Image>().sprite = sprite;
        }
        // check for info file
        System.IO.StreamReader infoFile =
                new System.IO.StreamReader(Application.streamingAssetsPath + 
                "/" + name + "/" + name + "Info.txt");
        string line = null;
        // create songNameText and destroy if already there
        if (songNameText != null)
            DestroyImmediate(songNameText);
        songNameText = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "SongNameText"));
        songNameText.transform.SetParent(GameObject.Find("SongMenuCanvas").transform, false);
        // edit songNameText if info file is given
        if ((line = infoFile.ReadLine()) != null)
        {
            songNameText.GetComponent<Text>().text = line;
            curSongName = line;
        }
        // create songDescription and destroy if already there
        if (songDescription != null)
            DestroyImmediate(songDescription);
        songDescription = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "SongDescription"));
        songDescription.transform.SetParent(GameObject.Find("SongMenuCanvas").transform, false);
        // edit songDescription
        string description = "";
        while ((line = infoFile.ReadLine()) != null)
        {
            description += line + "\n";
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


    public WWW fetchImageWWW(string path)
    {
        WWW ret = null;
        // check for png
        ret = new WWW(path + ".png");
        if (string.IsNullOrEmpty(ret.error))
        {
            return ret;
        }
        ret.Dispose();
        // check for jpg
        ret = new WWW(path + ".jpg");
        if (string.IsNullOrEmpty(ret.error))
        {
            return ret;
        }
        ret.Dispose();
        // return null otherwise
        Debug.Log("BAD IMAGE LOAD");
        return null;
    }
}
