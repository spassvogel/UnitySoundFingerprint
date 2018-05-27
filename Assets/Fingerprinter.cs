using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using System.Collections;
using System;
using SoundFingerprinting;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Audio;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Builder;
using System.IO;
using SoundFingerprinting.Query;

public class Fingerprinter : MonoBehaviour {
	
    public float trimCutoff = .01f;  	// Minimum volume of the clip before it gets trimmed    


	private AudioSource audioSource; 
	private string micName = null;
	private int sampleTime = 3;
	private int sampleRate = 44100;

	private float[] mAudioBuffer = null;

	private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
	private readonly IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library
	
	void Start() {
		StoreAudioFileFingerprintsInStorageForLaterRetrieval(Path.Combine(Application.streamingAssetsPath, "sweden.wav"));

		if (Microphone.devices == null || Microphone.devices.Length < 1) {
			Debug.LogError ("Microphone init error");
			return;
		}
		micName = Microphone.devices[0];

		audioSource = this.gameObject.AddComponent<AudioSource>();

		StartRecord();
	}
	
	public ResultEntry GetBestMatchForAudioClip(AudioClip clip)
	{	
		var audioSamples = AudioClipBridge.AudioClipToAudioSamples(clip);

		// query the underlying database for similar audio sub-fingerprints
		var queryResult = QueryCommandBuilder.Instance.BuildQueryCommand()
			.From(audioSamples)
			.UsingServices(modelService, audioService)
			.Query()
			.Result;
		
		return queryResult.BestMatch; // successful match has been found
	}


	public void StoreAudioFileFingerprintsInStorageForLaterRetrieval(string pathToAudioFile)
	{
		var track = new TrackData("unknown", "ARTIST", "TRACK", "ALBUM", 1988, 290);
		
		// store track metadata in the datasource
		var trackReference = modelService.InsertTrack(track);

		// create hashed fingerprints
		var hashedFingerprints = FingerprintCommandBuilder.Instance
			.BuildFingerprintCommand()
			.From(pathToAudioFile)
			.UsingServices(audioService)
			.Hash()
			.Result;
								
		// store hashes in the database for later retrieval
		modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
	}

	public bool StartRecord()
	{

		audioSource.clip = Microphone.Start (micName, false, 999, this.sampleRate);
		Invoke("StopRecord", sampleTime);

		return Microphone.IsRecording (micName);
	}


	public void StopRecord()
	{
		if (Microphone.IsRecording (micName)) {
			Microphone.End (micName);
		}

		AudioClip trimmedClip = SavWav.TrimSilence(audioSource.clip, trimCutoff);	
		if(trimmedClip == null)
		{
			print("Clip trimmed to 0");
			return;
		}       
		ResultEntry match = GetBestMatchForAudioClip(trimmedClip);
		
		if(match != null){
			Debug.Log("Found track " + match.Track.Title + " with confidence: " + match.Confidence);
		}
		else { 
			Debug.Log("Found NO match");
		}
        
		StartRecord();
	}
}
