using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Midi;

public class SongManager : MonoBehaviour {

    private const float EPSILON = 0.022f;
    private const float AUDIO_DELAY = 0.07f;
    private const float HIT_WINDOW = 0.1f;
    private const float ROLL_TIME = 2.4f;
    // note representations
    public class Note
    {
        public string value;       // what note is it; i.e. C#3, F3, E4, etc
        public float timestamp;    // moment in realtime when note is hit (in seconds)
        public GameObject rollNote;// 3d object representation of note
    };
    // MIDI stuff
    public MidiFile midi;
    public TempoEvent tempo;
    public List<Note> notes = new List<Note>();
    private List<Note> notesInWindow = new List<Note>();
    public int bpm;
    public int totalNotes;
    private float realStartTime;
    private int curIndex = 0;
    private int curRollIndex = 0;
    private GameObject roll;
    // Audio stuff
    private AudioSource[] audioSources;
    private AudioSource songAudio, bassAudio, snareAudio, hihatAudio, cymbalAudio;
    public DrumTriggerManager snareManager, hihatManager, crashManager, rideManager;

    // Use this for initialization
    void Start () {
        // init audio
        audioSources = GetComponents<AudioSource>();
        songAudio = audioSources[0];
        bassAudio = audioSources[1];
        snareAudio = audioSources[2];
        hihatAudio = audioSources[3];
        cymbalAudio = audioSources[4];
        midi = new MidiFile("Assets/SongData/uptownFunk/uptownFunkExpert.mid");
        tempo = null;
        bpm = 1;
        totalNotes = 0;
        foreach (MidiEvent note in midi.Events[0])
        {
            // at beginning, get bpm
            if (note is NAudio.Midi.TempoEvent)
            {
                tempo = (TempoEvent)note;
                double secondsPerQuarterNote = (double)tempo.MicrosecondsPerQuarterNote / 1000000;
                bpm = (int)(1 / secondsPerQuarterNote * 60);
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
                noteToAdd.timestamp = realTime;
                noteToAdd.rollNote = null;
                notes.Add(noteToAdd);
            }
        }
        Debug.Log("Total drum hits in song: " + totalNotes);
        realStartTime = Time.fixedTime + AUDIO_DELAY;
        // play all tracks
        songAudio.Play();
        bassAudio.Play();
        snareAudio.Play();
        hihatAudio.Play();
        cymbalAudio.Play();
        // get roll sheet
        roll = GameObject.Find("Roll");
    }


    // Update is called once per frame
    void Update() {
        float curTime = Time.fixedTime - realStartTime;
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
                curRollNote.rollNote.transform.Translate(new Vector3(.02f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1, 1);
            }
            else
            {
                curRollNote.rollNote.transform.Translate(new Vector3(.05f, 0.0f, 0.0f));
                curRollNote.rollNote.GetComponent<MeshRenderer>().material.color = new Color(1, 0.5f, 0, 1);
                Debug.Log("other note");
            }
            ++curRollIndex;
        }
        // move roll notes
        for (int i = 0; i < notes.Count; i++)
        {
            Note n = notes.ElementAt(i);

            if (n.rollNote != null)
            {
                float rollTick = 8.9f * Time.deltaTime / ROLL_TIME;
                Vector3 oldPos = n.rollNote.transform.localPosition;
                n.rollNote.transform.localPosition = new Vector3(oldPos.x, oldPos.y, oldPos.z - rollTick);
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
                if (isCrash(elem) &&
                    (crashManager.rightHit >= windowStart ||
                    crashManager.leftHit >= windowStart))
                {
                    cymbalAudio.volume = 1.0f;
                    DestroyImmediate(elem.rollNote);
                    notesInWindow.RemoveAt(i);
                }
                // ride
                if (isRide(elem) &&
                    (rideManager.rightHit >= windowStart ||
                    rideManager.leftHit >= windowStart))
                {
                    cymbalAudio.volume = 1.0f;
                    DestroyImmediate(elem.rollNote);
                    notesInWindow.RemoveAt(i);
                }
            }
        }
    }

    // helper boolean functions to determine what kind of hit it is
    bool isSnare(Note note)
    {
        return note.value == "D3" || note.value == "E3";
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
}
