using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Midi;

public class SongManager : MonoBehaviour {


    // constants
    private const float EPSILON = 0.022f;
    private const float AUDIO_DELAY = 0.07f;
    private const float HIT_WINDOW = 0.1f;
    private float ROLL_TIME = 1.5f;
    // note representations
    public class Note
    {
        public string value;       // what note is it; i.e. C#3, F3, E4, etc
        public float timestamp;    // moment in realtime when note is hit (in seconds)
        public GameObject rollNote;// 3d object representation of note
    };
    private List<GameObject> beatTicks = new List<GameObject>();
    // MIDI stuff
    public MidiFile midi;
    public TempoEvent tempo;
    public List<Note> notes = new List<Note>();
    private List<Note> notesInWindow = new List<Note>();
    private float secondsPerQuarterNote;
    private int numTicks = 0;
    public int bpm;
    public int totalNotes;
    private float realStartTime = 100.0f;
    private int curIndex = 0;
    private int curRollIndex = 0;
    private GameObject roll;
    // Audio stuff
    private float startBuffer;
    private bool startFlag = false;
    public string songName;
    public string difficulty = "Expert";
    private AudioSource[] audioSources;
    private AudioSource songAudio, bassAudio, snareAudio, hihatAudio, cymbalAudio, tomAudio;
    public DrumTriggerManager snareManager, hihatManager, crashManager, rideManager, highTomManager,
        medTomManager, lowTomManager;


    // Use this for initialization
    void Start () {
        // init audio
        audioSources = GetComponents<AudioSource>();
        songAudio = audioSources[0];
        songAudio.clip = (AudioClip)Resources.Load("SongData/" + songName + "/" + songName);
        bassAudio = audioSources[1];
        bassAudio.clip = (AudioClip)Resources.Load("SongData/" + songName + "/" + songName + "Bass");
        snareAudio = audioSources[2];
        snareAudio.clip = (AudioClip)Resources.Load("SongData/" + songName + "/" + songName + "Snare");
        hihatAudio = audioSources[3];
        hihatAudio.clip = (AudioClip)Resources.Load("SongData/" + songName + "/" + songName + "Hihat");
        cymbalAudio = audioSources[4];
        cymbalAudio.clip = (AudioClip)Resources.Load("SongData/" + songName + "/" + songName + "Cymbal");
        tomAudio = audioSources[5];
        tomAudio.clip = (AudioClip)Resources.Load("SongData/" + songName + "/" + songName + "Tom");
        fixNullTracks();
        // init midi
        midi = new MidiFile("Assets/Resources/SongData/" + songName + "/" + songName +
            difficulty + ".mid");
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
                bpm = (int)(1 / secondsPerQuarterNote * 60);
                startBuffer = secondsPerQuarterNote * 8;
                // use default roll time to find new one
                ROLL_TIME *= secondsPerQuarterNote * 4;
                Debug.Log("Playing song with bpm " + bpm);
            }
            // for actual note hits, report back information
            if (tempo != null && note.CommandCode == MidiCommandCode.NoteOn)
            {
                totalNotes++;
                NoteOnEvent tempNote = (NoteOnEvent)note;
                float quarterNoteOn = (float)note.AbsoluteTime / midi.DeltaTicksPerQuarterNote;
                float realTime = quarterNoteOn * 60 / bpm;
                Note noteToAdd = new global::SongManager.Note();
                noteToAdd.value = tempNote.NoteName;
                noteToAdd.timestamp = realTime + startBuffer;
                noteToAdd.rollNote = null;
                notes.Add(noteToAdd);
            }
        }
        Debug.Log("Total drum hits in song: " + totalNotes);
        realStartTime = Time.fixedTime + AUDIO_DELAY;
        // get roll sheet
        roll = GameObject.Find("Roll");
    }


    // Update is called once per frame
    void Update() {
        float curTime = Time.fixedTime - realStartTime;
        // take care of buffer at start
        if (!startFlag && curTime >= startBuffer)
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
        updateBeatTicks(curTime);
        // spawn roll notes
        Note curRollNote;
        while (curRollIndex < notes.Count
            && notes.ElementAt(curRollIndex).timestamp - ROLL_TIME < curTime )
        {
            curRollNote = notes.ElementAt(curRollIndex);
            // make a clone of RollNote prefab then make its parent the roll
            // this allows for relative transforms making it easy to move the notes
            curRollNote.rollNote = (GameObject)Instantiate(Resources.Load("RollNote"));
            curRollNote.rollNote.transform.SetParent(roll.transform, false);
            // move the notes horizontally based upon their 
            if (isSnare(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(-.05f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 1);
            }
            else if (isHiHat(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(-.02f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 0, 1);
            }
            else if (isCrash(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(-0.01f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 1);
            }
            else if (isRide(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(.06f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1, 1);
            }
            else if (isHighTom(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(0.0f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 1.0f, 1);
            }
            else if (isMedTom(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(0.02f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 0.5f, 1.0f, 1);
            }
            else if (isLowTom(curRollNote))
            {
                curRollNote.rollNote.transform.Translate(new Vector3(0.04f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 0.5f, 1);
            }
            else if (isBass((curRollNote))) {
                curRollNote.rollNote.transform.localScale = new Vector3(10.0f, 0.15f, 0.1f);
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.67f, 0.0f, 1);
            }
            ++curRollIndex;
        }
        // move roll notes
        for (int i = 0; i < notes.Count; i++)
        {
            Note n = notes.ElementAt(i);

            if (n.rollNote != null)
            {
                float rollTick = 9.96f * (curTime - n.timestamp) / ROLL_TIME + 4.98f;
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
            if (curTime - elem.timestamp > HIT_WINDOW / 2)
            {
                // snare
                if (isSnare(elem))
                {
                    snareAudio.volume = 0.0f;
                }
                // hihat
                if (isHiHat(elem))
                {
                    hihatAudio.volume = 0.0f;
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
                DestroyImmediate(elem.rollNote);
                notesInWindow.RemoveAt(i);
            }
            // note hit
            else
            {
                // snare
                if (isSnare(elem) &&
                    (snareManager.rightHit >= windowStart ||
                    snareManager.leftHit >= windowStart))
                {
                    snareAudio.volume = 1.0f;
                    DestroyImmediate(elem.rollNote);
                    notesInWindow.RemoveAt(i);
                }
                // hihat
                if (isHiHat(elem) &&
                    (hihatManager.rightHit >= windowStart ||
                    hihatManager.leftHit >= windowStart))
                {
                    hihatAudio.volume = 1.0f;
                    DestroyImmediate(elem.rollNote);
                    notesInWindow.RemoveAt(i);
                }
                // crash
                if ((isCrash(elem) &&
                    (crashManager.rightHit >= windowStart ||
                    crashManager.leftHit >= windowStart)) ||
                    (isRide(elem) &&
                    (rideManager.rightHit >= windowStart ||
                    rideManager.leftHit >= windowStart)))
                {
                    cymbalAudio.volume = 1.0f;
                    DestroyImmediate(elem.rollNote);
                    notesInWindow.RemoveAt(i);
                }
                // toms
                if ((isHighTom(elem) &&
                    (highTomManager.rightHit >= windowStart ||
                    highTomManager.leftHit >= windowStart))
                    ||
                    (isMedTom(elem) &&
                    (medTomManager.rightHit >= windowStart ||
                    medTomManager.leftHit >= windowStart))
                    ||
                    (isLowTom(elem) &&
                    (lowTomManager.rightHit >= windowStart ||
                    lowTomManager.leftHit >= windowStart)))
                {
                    Debug.Log("TOM HIT");
                    tomAudio.volume = 1.0f;
                    DestroyImmediate(elem.rollNote);
                    notesInWindow.RemoveAt(i);
                }
            }
        }
    }


    // helper boolean functions to determine what kind of hit it is
    bool isBass(Note note)
    {
        return note.value == "C3";
    }
    bool isSnare(Note note)
    {
        return note.value == "D3" || note.value == "E3" || note.value == "A#2";
    }
    bool isHiHat(Note note)
    {
        return note.value == "F#3" || note.value == "G#3" || note.value == "A#3";
    }
    bool isCrash(Note note)
    {
        return note.value == "C#4";
    }
    bool isRide(Note note)
    {
        return note.value == "D#4" || note.value == "F4";
    }
    bool isHighTom(Note note)
    {
        return note.value == "C4" || note.value == "D4";
    }
    bool isMedTom(Note note)
    {
        return note.value == "A3" || note.value == "B3";
    }
    bool isLowTom(Note note)
    {
        return note.value == "F3" || note.value == "G3";
    }


    // audio helper functions
    void updateBeatTicks(float curTime)
    {
        // spawn beat ticks on every quarter note
        if (curTime > numTicks * secondsPerQuarterNote)
        {
            beatTicks.Add((GameObject)Instantiate(Resources.Load("BeatTick")));
            beatTicks.ElementAt(beatTicks.Count-1).transform.SetParent(roll.transform, false);
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
            float rollTick = 9.96f * Time.deltaTime / ROLL_TIME;
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
    void fixNullTracks()
    {
        if (bassAudio.clip == null)
        {
            bassAudio.clip = songAudio.clip;
            bassAudio.volume = 0.0f;
        }
        if (snareAudio.clip == null)
        {
            snareAudio.clip = songAudio.clip;
            snareAudio.volume = 0.0f;
        }
        if (hihatAudio.clip == null)
        {
            hihatAudio.clip = songAudio.clip;
            hihatAudio.volume = 0.0f;
        }
        if (cymbalAudio.clip == null)
        {
            cymbalAudio.clip = songAudio.clip;
            cymbalAudio.volume = 0.0f;
        }
        if (tomAudio.clip == null)
        {
            tomAudio.clip = songAudio.clip;
            tomAudio.volume = 0.0f;
        }
    }
}
