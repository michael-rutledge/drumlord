using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Midi;

public class SongManager : MonoBehaviour {

    private const float EPSILON = 0.022f;
    private const float AUDIO_DELAY = 0.1f;
    private const float HIT_WINDOW = 0.1f;
    // struct to represent drum note
    public class Note
    {
        public string value;       // what note is it; i.e. C#3, F3, E4, etc
        public float timestamp;    // moment in realtime when note is hit (in seconds)
    };
    public MidiFile midi;
    public TempoEvent tempo;
    public List<Note> notes = new List<Note>();
    private List<Note> notesInWindow = new List<Note>();
    public int bpm;
    public int totalNotes;
    private float realStartTime;
    private int curIndex = 0;
    private AudioSource[] audioSources;
    private AudioSource songAudio, bassAudio, snareAudio, hihatAudio, cymbalAudio;
    public DrumTriggerManager snareManager, hihatManager, crashManager, rideManager;

    // Use this for initialization
    void Start () {
        // init drum managers
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
            curIndex++;
            Debug.Log("curTime: " + curTime + ", timestamp: " + curNote.timestamp);
            // add note to notes currently in hit window
            notesInWindow.Add(curNote);
        }

        // go trough notes currently in hit window
        for (int i = 0; i < notesInWindow.Count; i++)
        {
            Note elem = notesInWindow.ElementAt(i);
            float windowStart = elem.timestamp - HIT_WINDOW / 2;
            // note missed
            if (curTime - elem.timestamp > HIT_WINDOW / 2)
            {
                // snare
                if (elem.value == "D3" || elem.value == "E3")
                {
                    snareAudio.volume = 0.0f;
                }
                // hihat
                if (elem.value == "F#3" || elem.value == "G#3" || elem.value == "A#3")
                {
                    hihatAudio.volume = 0.0f;
                }
                // hihat
                if (elem.value == "C#4" || elem.value == "D#4" || elem.value == "F4")
                {
                    cymbalAudio.volume = 0.0f;
                }
                notesInWindow.RemoveAt(i);
            }
            // note hit
            else
            {
                // snare
                if ((elem.value == "D3" || elem.value == "E3") &&
                    (snareManager.rightHit >= windowStart ||
                    snareManager.leftHit >= windowStart))
                {
                    snareAudio.volume = 1.0f;
                    notesInWindow.RemoveAt(i);
                }
                // hihat
                if ((elem.value == "F#3" || elem.value == "G#3" || elem.value == "A#3") &&
                    (hihatManager.rightHit >= windowStart ||
                    hihatManager.leftHit >= windowStart))
                {
                    hihatAudio.volume = 1.0f;
                    notesInWindow.RemoveAt(i);
                }
                // crash
                if ((elem.value == "C#4") &&
                    (crashManager.rightHit >= windowStart ||
                    crashManager.leftHit >= windowStart))
                {
                    cymbalAudio.volume = 1.0f;
                    notesInWindow.RemoveAt(i);
                }
                // ride
                if ((elem.value == "D#4" || elem.value == "F4") &&
                    (rideManager.rightHit >= windowStart ||
                    rideManager.leftHit >= windowStart))
                {
                    cymbalAudio.volume = 1.0f;
                    notesInWindow.RemoveAt(i);
                }
            }
        }
    }
}
