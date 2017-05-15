using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Midi;
using UnityEngine.SceneManagement;

public class SongManager : MonoBehaviour {


    // constants
    private const float EPSILON = 0.022f;
    private const float AUDIO_DELAY = 0.1f;
    private const float HIT_WINDOW = 0.22f;
    private const string PREFAB_DIR = "Prefabs/";
    private const string SONG_DIR = "SongData/";
    private const string fPrefix = "file://";
    private float ROLL_TIME = 1.5f;
    // note representations
    public class Note
    {
        public string value;                    // what note is it; i.e. C#3, F3, E4, etc
        public float timestamp;                 // moment in realtime when note is hit (in seconds)
        public GameObject rollNote;             // 3d object representation of note
        public GameObject hitParticles;         // particle system for hits
        public int state;                       // 0 = normal, 1 = hit, 2 = miss
    };
    private List<GameObject> beatTicks = new List<GameObject>();
    // MIDI stuff
    public MidiFile midi;
    public TempoEvent tempo;
    public List<Note> notes = new List<Note>();
    private List<Note> notesInWindow = new List<Note>();
    private List<GameObject> particlesOut = new List<GameObject>();
    private float secondsPerQuarterNote;
    private int numTicks = 0;
    public int bpm;
    public int totalNotes;
    public float realStartTime = 100.0f;
    private int curIndex = 0;
    private int curRollIndex = 0;
    private List<int> rollNoteIndeces = new List<int>();
    private GameObject roll;
    //multiplier stuff
    private int streak = 0;
    private int multiplier = 0;
    //score stuff
    private int score = 0;
    private int highStreak = 0;
    private int notesMissed = 0;
    private int notesHit = 0;
    public GameObject multiplierText;
    public Color multiplierWhite = new Color(1, 1, 1, 1);
    public Color multiplierGreen = new Color(0.43f, 0.68f, 0.33f, 1);
    public Color multiplierBlue = new Color(0.10f, 0.59f, 0.88f, 1);
    public Color multiplierPink = new Color(1.00f, 0.05f, 0.72f, 1);
    public Color multiplierPurple = new Color(0.62f, 0.00f, 1.00f, 1);
    public GameObject streakText;
    public GameObject scoreText;
    public GameObject streakPopup;
    private bool bassAvailable = false;
    private KeyCode bassKey;
    public bool bassDown;
    // Audio stuff
    private float startBuffer;
    public float curTime;
    private bool startFlag = false;
    private string songName;
    private AudioSource[] audioSources;
    private AudioSource songAudio, bassAudio, snareAudio, hihatAudio, cymbalAudio, tomAudio;
    public DrumTriggerManager snareManager, hihatManager, crashManager, rideManager, highTomManager,
        medTomManager, lowTomManager;


    // Use this for initialization
    void Start () {
        // pass selected song id from menu
        songName = ApplicationModel.selectedSongId;
        bassAvailable = ApplicationModel.bassAvailable;
        bassKey = ApplicationModel.bassKey;
        Debug.Log("Bass available: " + bassAvailable + " key: " + bassKey);
        // init audio
        string prefix = fPrefix + Application.streamingAssetsPath + "/" + songName + "/";
        audioSources = GetComponents<AudioSource>();
        // for each possible track, fetch the audio file from streaming assets directory
        songAudio = audioSources[0];
        StartCoroutine(fetchAudioWWW(prefix + songName, songAudio));
        bassAudio = audioSources[1];
        StartCoroutine(fetchAudioWWW(prefix + songName + "Bass", bassAudio));
        snareAudio = audioSources[2];
        StartCoroutine(fetchAudioWWW(prefix + songName + "Snare", snareAudio));
        hihatAudio = audioSources[3];
        StartCoroutine(fetchAudioWWW(prefix + songName + "Hihat", hihatAudio));
        cymbalAudio = audioSources[4];
        StartCoroutine(fetchAudioWWW(prefix + songName + "Cymbal", cymbalAudio));
        tomAudio = audioSources[5];
        StartCoroutine(fetchAudioWWW(prefix + songName + "Tom", tomAudio));
        // init midi
        midi = new MidiFile(Application.streamingAssetsPath + "/" + songName + "/" + songName + ".mid");
        tempo = null;
        bpm = 1;
        totalNotes = 0;
        foreach (MidiEvent note in midi.Events[0])
        {
            // at beginning, get bpm
            if (note is NAudio.Midi.TempoEvent)
            {
                tempo = (TempoEvent)note;
                secondsPerQuarterNote = (float)tempo.MicrosecondsPerQuarterNote / 1000000;
                bpm = (int)Mathf.Round(1 / secondsPerQuarterNote * 60);
                startBuffer = secondsPerQuarterNote * 8;
                // use default roll time to find new one
                ROLL_TIME *= secondsPerQuarterNote * 2;
                Debug.Log("Playing song with bpm " + bpm);
            }
            // for actual note hits, report back information
            if (tempo != null && note.CommandCode == MidiCommandCode.NoteOn)
            {
                totalNotes++;
                NoteOnEvent tempNote = (NoteOnEvent)note;
                // check what quarter note we are on, use that to calculate realTime
                float quarterNoteOn = (float)note.AbsoluteTime / midi.DeltaTicksPerQuarterNote;
                float realTime = quarterNoteOn * 60 / bpm;
                // create the note and assign its data
                if (noteInDifficulty(tempNote, quarterNoteOn) && (bassAvailable || tempNote.NoteName != "C3"))
                {
                    Note noteToAdd = new global::SongManager.Note();
                    noteToAdd.value = tempNote.NoteName;
                    noteToAdd.timestamp = realTime + startBuffer;
                    noteToAdd.rollNote = null;
                    notes.Add(noteToAdd);
                }
            }
        }
        Debug.Log("Total drum hits in song: " + totalNotes);
        realStartTime = Time.fixedTime + AUDIO_DELAY;
        // get roll sheet
        roll = GameObject.Find("Roll");
    }


    // Update is called once per frame
    void Update() {
        curTime = Time.fixedTime - realStartTime;
        // take care of buffer at start
        if (!startFlag && curTime >= startBuffer - AUDIO_DELAY)
        {
            // play all tracks
            songAudio.Play();
            bassAudio.Play();
            snareAudio.Play();
            hihatAudio.Play();
            cymbalAudio.Play();
            tomAudio.Play();
            startFlag = true;
        }
        Note curNote;
        // for every note that is currently active, do something
        while (curIndex < notes.Count
            && notes.ElementAt(curIndex).timestamp - HIT_WINDOW / 2 < curTime + EPSILON
            && notes.ElementAt(curIndex).timestamp - HIT_WINDOW / 2 > curTime - EPSILON)
        {
            // right now, all we are doing is printing out the note's data
            curNote = notes.ElementAt(curIndex);
            ++curIndex;
            // add note to notes currently in hit window
            notesInWindow.Add(curNote);
        }
        updateBeatTicks();
        // spawn roll notes
        Note curRollNote;
        while (curRollIndex < notes.Count
            && notes.ElementAt(curRollIndex).timestamp - ROLL_TIME < curTime)
        {
            curRollNote = notes.ElementAt(curRollIndex);
            rollNoteIndeces.Add(curRollIndex);
            // make a clone of RollNote prefab then make its parent the roll
            // this allows for relative transforms making it easy to move the notes
            curRollNote.rollNote = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "RollNote"));
            curRollNote.rollNote.transform.SetParent(roll.transform, false);
            curRollNote.rollNote.GetComponent<MeshRenderer>().material =
                Resources.Load("Materials/rollNoteOpaque") as Material;
            // move the notes horizontally based upon their 
            if (isHiHat(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(-.02f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.hiHatColor;
            }
            else if (isSnare(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(-.05f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.snareColor;
            }
            else if (isCrash(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(-0.01f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.crashColor;
            }
            else if (isRide(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(.06f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.rideColor;
            }
            else if (isHighTom(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(0.0f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.highTomColor;
            }
            else if (isMedTom(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(0.02f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.medTomColor;
            }
            else if (isLowTom(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(0.04f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.lowTomColor;
            }
            else if (isBass((curRollNote))) {
                curRollNote.rollNote.transform.localScale = new Vector3(10.0f, 0.15f, 0.1f);
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color =
                    ApplicationModel.bassColor;
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.renderQueue = 3999;
            }
            ++curRollIndex;
        }
        // move roll notes
        for (int i = 0; i < rollNoteIndeces.Count; i++)
        {
            Note n = notes.ElementAt(rollNoteIndeces.ElementAt(i));

            if (n.rollNote != null && n.state != 1)
            {
                float rollTick = 9.7f * (curTime - n.timestamp) / ROLL_TIME + 4.85f;
                Vector3 oldPos = n.rollNote.transform.localPosition;
                n.rollNote.transform.localPosition = new Vector3(oldPos.x, oldPos.y, -rollTick);
            }
        }

        // go trough notes currently in hit window to for hit detection
        for (int i = 0; i < notesInWindow.Count; i++)
        {
            Note elem = notesInWindow.ElementAt(i);
            float windowStart = elem.timestamp - HIT_WINDOW / 2;
            // note missed
            if (curTime - elem.timestamp > HIT_WINDOW / 2 && elem.state == 0)
            {
                streak = 0;
                multiplier = 1;
                notesMissed++;
                // update text
                multiplierText.GetComponent<TextMesh>().color = multiplierWhite;
                streakText.GetComponent<TextMesh>().color = new Color(0.26f, 0.26f, 0.26f);
                multiplierText.GetComponent<TextMesh>().text = "Multiplier: x" + multiplier;
                streakText.GetComponent<TextMesh>().text = "Streak: " + streak;
                // hihat
                if (isHiHat(elem))
                {
                    hihatAudio.volume = 0.0f;
                }
                // snare
                if (isSnare(elem))
                {
                    snareAudio.volume = 0.0f;
                }
                // crash and ride
                if (isCrash(elem) || isRide(elem))
                {
                    cymbalAudio.volume = 0.0f;
                }
                // toms
                if (isHighTom(elem) || isMedTom(elem) || isLowTom(elem))
                {
                    tomAudio.volume = 0.0f;
                }
                elem.state = 2;
            }
            // note miss fade out
            else if (elem.state == 2)
            {
                float colorD = Time.deltaTime * 5;
                Color oldColor = elem.rollNote.GetComponent<MeshRenderer>().material.color;
                // change material to fading material at first
                if (elem.rollNote.GetComponent<MeshRenderer>().material.color.a == 1.0f)
                {
                    elem.rollNote.GetComponent<MeshRenderer>().material =
                        Resources.Load("Materials/rollNoteFade") as Material;
                }
                elem.rollNote.GetComponent<MeshRenderer>().material.color = 
                    new Color(oldColor.r - colorD, oldColor.g - colorD, oldColor.b - colorD, oldColor.a - colorD);
                if (elem.rollNote.GetComponent<MeshRenderer>().material.color.a <= 0)
                {
                    DestroyImmediate(elem.rollNote);
                    rollNoteIndeces.RemoveAt(i);
                    notesInWindow.RemoveAt(i);
                }
            }
            // note hit
            else if (elem.state == 0)
            {
                // hihat
                if (isHiHat(elem) &&
                    (hihatManager.rightHit >= windowStart ||
                    hihatManager.leftHit >= windowStart))
                {
                    hitDrum(hihatManager, hihatAudio, elem, i);
                }
                // snare
                if (isSnare(elem) &&
                    (snareManager.rightHit >= windowStart ||
                    snareManager.leftHit >= windowStart))
                {
                    hitDrum(snareManager, snareAudio, elem, i);
                }
                // cymbals
                if (isCrash(elem) &&
                    (crashManager.rightHit >= windowStart ||
                    crashManager.leftHit >= windowStart))
                {
                    hitDrum(crashManager, cymbalAudio, elem, i);
                }
                if (isRide(elem) &&
                    (rideManager.rightHit >= windowStart ||
                    rideManager.leftHit >= windowStart))
                {
                    hitDrum(rideManager, cymbalAudio, elem, i);
                }
                // toms
                if ((isHighTom(elem) &&
                    (highTomManager.rightHit >= windowStart ||
                    highTomManager.leftHit >= windowStart)))
                {
                    hitDrum(highTomManager, tomAudio, elem, i);
                }
                if (isMedTom(elem) &&
                    (medTomManager.rightHit >= windowStart ||
                    medTomManager.leftHit >= windowStart))
                {
                    hitDrum(medTomManager, tomAudio, elem, i);
                }
                if (isLowTom(elem) &&
                    (lowTomManager.rightHit >= windowStart ||
                    lowTomManager.leftHit >= windowStart))
                {
                    hitDrum(lowTomManager, tomAudio, elem, i);
                }
                if (isBass(elem) && Input.inputString != "" && Input.GetKeyDown(bassKey) && !bassDown)
                {
                    hitDrum(null, bassAudio, elem, i);
                }
            }
            // Note hit change color
            else if (elem.state == 1)
            {
                float colorD = Time.deltaTime * 10;
                Color oldColor = elem.rollNote.GetComponent<MeshRenderer>().material.color;
                // change material to fading material at first
                if (elem.rollNote.GetComponent<MeshRenderer>().material.color.a == 1.0f)
                {
                    elem.rollNote.GetComponent<MeshRenderer>().material =
                        Resources.Load("Materials/rollNoteFade") as Material;
                }
                Vector3 oldScale = elem.rollNote.transform.localScale;
                // change color to brighten and fade away
                elem.rollNote.GetComponent<MeshRenderer>().material.color =
                    new Color(oldColor.r + colorD, oldColor.g + colorD, oldColor.b + colorD, oldColor.a - colorD);
                // change scale to get bigger as the note fades away
                elem.rollNote.transform.localScale = new Vector3(oldScale.x * (1 + Time.deltaTime * 8), oldScale.y * (1 +  Time.deltaTime * 8), 1);
                if (elem.rollNote.GetComponent<MeshRenderer>().material.color.a <= 0)
                {
                    DestroyImmediate(elem.rollNote);
                    rollNoteIndeces.RemoveAt(i);
                    notesInWindow.RemoveAt(i);
                }
            }
            else
            {
                Debug.Log("FUCKED UP");
            }
        }
        bassDown = Input.GetKeyDown(bassKey);
        updateTextAnimations();
        // endgame logic
        if ((startFlag && !songAudio.isPlaying) || songAudio == null)
        {
            ApplicationModel.score = score;
            ApplicationModel.highStreak = highStreak;
            ApplicationModel.notesMissed = notesMissed;
            ApplicationModel.percentHit = (int) (((float)notesHit / (notesMissed + notesHit))*100);
            SceneManager.LoadScene("resultScreen");
        }
    }


    // helper boolean functions to determine what kind of hit it is
    private bool isBass(Note note)
    {
        return note.value == "C3";
    }
    private bool isSnare(Note note)
    {
        return note.value == "D3" || note.value == "E3" || note.value == "A#2";
    }
    private bool isHiHat(Note note)
    {
        return note.value == "F#3" || note.value == "G#3" || note.value == "A#3";
    }
    private bool isCrash(Note note)
    {
        return note.value == "C#4";
    }
    private bool isRide(Note note)
    {
        return note.value == "D#4" || note.value == "F4";
    }
    private bool isHighTom(Note note)
    {
        return note.value == "C4" || note.value == "D4";
    }
    private bool isMedTom(Note note)
    {
        return note.value == "A3" || note.value == "B3";
    }
    private bool isLowTom(Note note)
    {
        return note.value == "F3" || note.value == "G3";
    }


    // audio helper functions
    private void updateBeatTicks()
    {
        // spawn beat ticks on every quarter note
        if (curTime > numTicks * secondsPerQuarterNote)
        {
            beatTicks.Add((GameObject)Instantiate(Resources.Load(PREFAB_DIR + "BeatTick")));
            beatTicks.ElementAt(beatTicks.Count-1).transform.SetParent(roll.transform, false);
            // render behind roll notes
            beatTicks.ElementAt(beatTicks.Count - 1).GetComponent<MeshRenderer>().material.renderQueue = 3998;
            // every other beattick is bigger
            if (numTicks % 2 != 0)
            {
                beatTicks.ElementAt(beatTicks.Count - 1).transform.localScale =
                    new Vector3(1.0f, 0.016f, 0.004f);
            }
            numTicks++;
        }
        // move beat ticks
        for (int i = 0; i < beatTicks.Count; i++)
        {
            float rollTick = 9.7f * Time.deltaTime / ROLL_TIME;
            Vector3 oldPos = beatTicks.ElementAt(i).transform.localPosition;
            beatTicks.ElementAt(i).transform.localPosition = new Vector3(oldPos.x, oldPos.y, oldPos.z - rollTick);
            // delete beat tick if under the roll
            if (beatTicks.ElementAt(i).transform.localPosition.z < -5.0f)
            {
                DestroyImmediate(beatTicks.ElementAt(i));
                beatTicks.RemoveAt(i);
            }
        }
    }

    private void updateTextAnimations()
    {
        float dt = Time.fixedDeltaTime;
        // multiplier text animations
        if (multiplierText.transform.localScale.x > 0.5f)
        {
            Vector3 oldScale = multiplierText.transform.localScale;
            multiplierText.transform.localScale = new Vector3(oldScale.x - 1.5f * Time.fixedDeltaTime, oldScale.y, oldScale.z);
        }
        if (streakText.transform.localScale.x > 0.5f)
        {
            Vector3 oldScale = streakText.transform.localScale;
            streakText.transform.localScale = new Vector3(oldScale.x - 0.5f * Time.fixedDeltaTime, oldScale.y, oldScale.z);
        }
        if ((streak == 50 || streak % 100 == 0) && streak > 0)
        {
            streakPopup.GetComponent<TextMesh>().text = streak + " NOTE STREAK!!!";
            streakPopup.GetComponent<TextMesh>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            streakPopup.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        if (streakPopup.GetComponent<TextMesh>().color.a > 0)
        {
            Vector3 oldScale = streakPopup.transform.localScale;
            streakPopup.transform.localScale = new Vector3(oldScale.x + Mathf.Cos(Time.fixedDeltaTime * 20) * .0025f, oldScale.y + Mathf.Cos(Time.fixedDeltaTime * 20) * .0025f, oldScale.z);
            Color previousStreakPopup = streakPopup.GetComponent<TextMesh>().color;
            streakPopup.GetComponent<TextMesh>().color = new Color(previousStreakPopup.r, previousStreakPopup.g, previousStreakPopup.b, previousStreakPopup.a - Time.fixedDeltaTime);
        }
    }


    private void hitDrum(DrumTriggerManager dm, AudioSource source, Note elem, int i)
    {
        // set location to right on the beatline
        Vector3 oldPos = elem.rollNote.transform.localPosition;
        elem.rollNote.transform.localPosition = new Vector3(oldPos.x, oldPos.y, -4.85f);
        // particle effects
        if (isBass(elem))
        {
            elem.hitParticles = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "BassHitParticles"));
            ParticleSystem ps = elem.hitParticles.GetComponent<ParticleSystem>();
            elem.hitParticles.transform.SetParent(roll.transform, false);
            elem.hitParticles.transform.localPosition = elem.rollNote.transform.localPosition;
            elem.hitParticles.transform.localPosition += new Vector3(5f, 0, 0);
            particlesOut.Add(elem.hitParticles);
            ps.Play();

            elem.hitParticles = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "BassHitParticles"));
            ParticleSystem ps2 = elem.hitParticles.GetComponent<ParticleSystem>();
            elem.hitParticles.transform.SetParent(roll.transform, false);
            elem.hitParticles.transform.Rotate(0, -180, 0);
            elem.hitParticles.transform.localPosition = elem.rollNote.transform.localPosition;
            elem.hitParticles.transform.localPosition += new Vector3(-5f, 0, 0);
            particlesOut.Add(elem.hitParticles);
            ps2.Play();
        }
        else
        {
            elem.hitParticles = (GameObject)Instantiate(Resources.Load(PREFAB_DIR + "NoteHitParticles"));
            ParticleSystem ps = elem.hitParticles.GetComponent<ParticleSystem>();
            elem.hitParticles.transform.SetParent(roll.transform, false);
            elem.hitParticles.transform.localPosition = elem.rollNote.transform.localPosition;
            ps.startColor = elem.rollNote.GetComponent<MeshRenderer>().material.color;
            particlesOut.Add(elem.hitParticles);
            ps.Play();
        }
        // get rid of dead particles
        for (int idx = 0; idx < particlesOut.Count; idx++)
        {
            if (!particlesOut.ElementAt(idx).GetComponent<ParticleSystem>().IsAlive())
            {
                DestroyImmediate(particlesOut.ElementAt(idx));
                particlesOut.RemoveAt(idx);
            }
        }
        // deal with controller logic to avoid double hits
        if (dm != null)
        {
            dm.rightHit = 0.0f;
            dm.leftHit = 0.0f;
        }
        // deal with audio
        source.volume = 1.0f;
        elem.state = 1;
        // deal with score
        streak++;
        highStreak = streak > highStreak ? streak : highStreak;
        notesHit++;
        multiplier = streak / 10 + 1;
        multiplier = (multiplier > 5) ? 5 : multiplier;
        if (streak > 0)
            score += multiplier * 10;
        // update multiplierText color
        switch (multiplier)
        {
            case 2:
                multiplierText.GetComponent<TextMesh>().color = multiplierGreen;
                break;
            case 3:
                multiplierText.GetComponent<TextMesh>().color = multiplierBlue;
                break;
            case 4:
                multiplierText.GetComponent<TextMesh>().color = multiplierPink;
                break;
            case 5:
                multiplierText.GetComponent<TextMesh>().color = multiplierPurple;
                break;
            default:
                multiplierText.GetComponent<TextMesh>().color = multiplierWhite;
                break;
        }
        // update streakText color
        float s = 66.0f;
        float sr = 159.0f, sg = 143.0f, sb = -66.0f;
        float sAmount = streak > 40 ? 40 : streak;
        streakText.GetComponent<TextMesh>().color = new Color((s + sr * sAmount / 40.0f) / 255.0f,
            (s + sg * sAmount / 40.0f) / 255.0f, (s + sb * sAmount / 40.0f)/ 255.0f);
        // update text scale
        if (streak > 0 && streak % 10 == 0)
        {
            multiplierText.transform.localScale = new Vector3(0.7f, 0.5f, 0.5f);
        }
        streakText.transform.localScale = new Vector3(0.55f, 0.5f, 0.5f);
        // update text
        multiplierText.GetComponent<TextMesh>().text = "Multiplier: x" + multiplier;
        streakText.GetComponent<TextMesh>().text = "Streak: " + streak;
        scoreText.GetComponent<TextMesh>().text = "Score: " + score;
    }

    private IEnumerator fetchAudioWWW(string path, AudioSource source)
    {
        Debug.Log("Running fetchAudioWWW...\n");
        WWW ret = null;
        // check for wav by default, but change to ogg if necessary
        string suffix = ".wav";
        if (System.IO.File.Exists(path.Substring(7) + ".ogg"))
        {
            suffix = ".ogg";
        }
        // check for wav
        ret = new WWW(path + suffix);
        // wait for download
        yield return ret;
        if (string.IsNullOrEmpty(ret.error))
        {
            source.clip = ret.GetAudioClip(false, false);
        }
        else
        {
            Debug.LogError(ret.error);
        }
    }


    // midi helper functions
    private bool noteInDifficulty(NoteOnEvent tempNote, float qn)
    {
        // quantify difficulty to make filtering notes easier, expert by default
        int diffIndex = 3;
        switch (ApplicationModel.difficulty)
        {
            case "Hard":
                diffIndex = 2;
                break;
            case "Medium":
                diffIndex = 1;
                break;
            case "Easy":
                diffIndex = 0;
                break;
            default:
                break;
        }
        // by default and in expert mode, let all notes in
        bool ret = true;
        // Hard: prevent bass kicks that aren't on quarter notes
        if (diffIndex <= 2)
            ret &= !( tempNote.NoteName == "C3" && !modeqFloat(1.0f, qn) );
        // Medium: prevent notes that arent at least 8th note complexity
        if (diffIndex <= 1)
            ret &= ( modeqFloat(1.0f, qn) || modeqFloat(0.5f, qn) ) && tempNote.NoteName != "C3";
        // Easy:
        if (diffIndex <= 0)
            ret &= modeqFloat(1.0f, qn);
        return ret;
    }
    private bool modeqFloat(float n, float qn)
    {
        int nInt = (int)(n * 100);
        int qInt = (int)(qn * 100);
        nInt %= 100;
        qInt %= 100;
        return Mathf.Abs(nInt - qInt) <= 10;
    }
}
 