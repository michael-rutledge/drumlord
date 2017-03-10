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
    private const float EPSILON = 0.01f;

	// Use this for initialization
	void Start () {
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
        realStartTime = Time.fixedTime;
    }

    // Update is called once per frame
    void Update () {
        float curTime = Time.fixedTime;
        // for every note that is currently active, do something
		while (curIndex < notes.Count
            && notes.ElementAt(curIndex).timestamp < curTime - realStartTime + EPSILON
            && notes.ElementAt(curIndex).timestamp > curTime - realStartTime - EPSILON)
        {
            // right now, all we are doing is printing out the note's data
            Note curNote = notes.ElementAt(curIndex);
            Debug.Log("Note " + curNote.value + "hit at time " + curNote.timestamp);
            curIndex++;
        }
	}
}
