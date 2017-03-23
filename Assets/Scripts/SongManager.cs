using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Midi;

public class SongManager : MonoBehaviour {

    // struct to represent drum note
    public class Note
    {
        public string value;       // what note is it; i.e. C#3, F3, E4, etc
        public float timestamp;    // moment in realtime when note is hit (in seconds)
    };
    public MidiFile midi;
    public TempoEvent tempo;
    public List<Note> notes = new List<Note>();
    public int bpm;
    public int totalNotes;
    private float realStartTime;
    private int curIndex = 0;
    private AudioSource[] audioSources;
    private AudioSource songAudio, bassAudio, snareAudio, hihatAudio, cymbalAudio;
    private const float EPSILON = 0.022f;
    private const float AUDIO_DELAY = 0.1f;
    private const float HIT_WINDOW = 0.1f;
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
        midi = new MidiFile("Assets/SongData/UptownFunk/uptownFunkExpert.mid");
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
    void Update () {
        float curTime = Time.fixedTime - realStartTime;
        Note curNote;
        // for every note that is currently active, do something
        while (curIndex < notes.Count
            && notes.ElementAt(curIndex).timestamp + HIT_WINDOW/2 < curTime + EPSILON
            && notes.ElementAt(curIndex).timestamp + HIT_WINDOW/2 > curTime - EPSILON)
        {
            // right now, all we are doing is printing out the note's data
            curNote = notes.ElementAt(curIndex);
            //Debug.Log("Note " + curNote.value + ", " + curNote.timestamp + " hit at time "
            //    + curTime);
            curIndex++;
            SteamVR_Controller.Input((int)snareManager.rightHand.index).TriggerHapticPulse((ushort)3999);
            // snare hit detection
            if (curNote.value == "D3" || curNote.value == "E3")
            {
                Debug.Log("curTime: " + curTime + ", rightHit: " + snareManager.rightHit);
                if (curTime - snareManager.rightHit <= HIT_WINDOW ||
                    curTime - snareManager.leftHit <= HIT_WINDOW)
                {
                    snareAudio.volume = 1.0f;
                }
                else
                {
                    snareAudio.volume = 0.0f;
                }
            }
            // hihat hit detection
            if (curNote.value == "F#3" || curNote.value == "G#3" || curNote.value == "A#3")
            {
                Debug.Log("curTime: " + curTime + ", rightHit: " + hihatManager.rightHit);
                if (curTime - hihatManager.rightHit <= HIT_WINDOW ||
                    curTime - hihatManager.leftHit <= HIT_WINDOW)
                {
                    hihatAudio.volume = 1.0f;
                }
                else
                {
                    hihatAudio.volume = 0.0f;
                }
            }
            // crash hit detection
            if (curNote.value == "C#4")
            {
                Debug.Log("curTime: " + curTime + ", rightHit: " + crashManager.rightHit);
                if (curTime - crashManager.rightHit <= HIT_WINDOW ||
                    curTime - crashManager.leftHit <= HIT_WINDOW)
                {
                    cymbalAudio.volume = 1.0f;
                }
                else
                {
                    cymbalAudio.volume = 0.0f;
                }
            }
        }
    }
}
