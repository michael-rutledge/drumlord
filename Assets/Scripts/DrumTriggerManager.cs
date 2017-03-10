﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;

public class DrumTriggerManager : MonoBehaviour {

    // SteamVR data
    public SteamVR_TrackedObject rightHand;
    private SteamVR_Controller.Device rightDevice;
    public SteamVR_TrackedObject leftHand;
    private SteamVR_Controller.Device leftDevice;
    // Time data
    private double rightHit;
    private double leftHit;
    private double startTime;
    // Position data
    public GameObject stickRightHead;
    public GameObject stickLeftHead;
    private Vector3 oldRightPos;
    private Vector3 oldLeftPos;
    // Audio data
    private AudioSource audio;

    // Use this for initialization
    void Start () {
        audio = GetComponent<AudioSource>();
        rightHit = leftHit = -1.0;
        startTime = Time.fixedTime;
        oldRightPos = stickRightHead.transform.position;
        oldLeftPos = stickLeftHead.transform.position;
        // START DEBUG MIDI PRINTING
        MidiFile midi = new MidiFile("Assets/SongData/UptownFunk/uptownFunkExpert.mid");
        TempoEvent tempo = null;
        int bpm = 1;
        int totalNotes = 0;
        foreach (MidiEvent note in midi.Events[0])
        {
            // at beginning, get bpm
            if (note is NAudio.Midi.TempoEvent)
            {
                tempo = (TempoEvent)note;
                double secondsPerQuarterNote = (double)tempo.MicrosecondsPerQuarterNote / 1000000;
                bpm = (int)(1 / secondsPerQuarterNote * 60);
                Debug.Log("Tempo Event with microSecondsPerQuarterNote " + bpm);
            }
            // for actual note hits, report back information
            if (tempo != null && note.CommandCode == MidiCommandCode.NoteOn)
            {
                totalNotes++;
                NoteOnEvent tempNote = (NoteOnEvent)note;
                float quarterNoteOn = (float)note.AbsoluteTime / midi.DeltaTicksPerQuarterNote;
                float realTime = quarterNoteOn * 60 / bpm;
                Debug.Log("Note of " + tempNote.NoteName + " on at " + realTime);
            }
        }
        Debug.Log("Total drum hits in uptown funk: " + totalNotes);
        // END DEBUG MIDI PRINTING
    }
	
	// Update is called once per frame
	void Update () {
        rightDevice = SteamVR_Controller.Input((int)rightHand.index);
        leftDevice = SteamVR_Controller.Input((int)leftHand.index);
        oldRightPos = stickRightHead.transform.position;
        oldLeftPos = stickLeftHead.transform.position;
    }

    // To be called when other objects collide
    void OnTriggerEnter(Collider other)
    {
        float curTime = Time.fixedTime;
        // Check if right stick hits drum
        if (other.gameObject.name.Equals("StickRightHead") &&
            oldRightPos.y > stickRightHead.transform.position.y)
        {
            //audio.Play();
            SteamVR_Controller.Input((int)rightHand.index).TriggerHapticPulse((ushort)2000);
            float angM = rightDevice.angularVelocity.magnitude;
            Debug.Log(this.name + " collision with " + other.gameObject.name +
                " with velocity " + angM + " at time " + curTime);
            rightHit = curTime - startTime;
        }
        // Check if left stick hits drum
        if (other.gameObject.name.Equals("StickLeftHead") &&
            oldLeftPos.y > stickLeftHead.transform.position.y)
        {
            //audio.Play();
            SteamVR_Controller.Input((int)leftHand.index).TriggerHapticPulse((ushort)2000);
            float angM = leftDevice.angularVelocity.magnitude;
            Debug.Log(this.name + " collision with " + other.gameObject.name +
                " with velocity " + angM + " at time " + curTime);
            leftHit = curTime - startTime;
        }
    }

    // To be called when colliding objects leave contact
    void OnTriggerExit(Collider other)
    {

    }

    void OnGUI() {
        GUI.Label( new Rect(450, 100, 100, 100),
            ("" + stickRightHead.transform.position.y) );
    }
}
