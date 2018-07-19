using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SoundFingerprinting;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Audio;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Builder;
using System.IO;
using SoundFingerprinting.Query;
using System.Text.RegularExpressions;

public class Fingerprinter : MonoBehaviour {
	
    public float trimCutoff = .01f;  	// Minimum volume of the clip before it gets trimmed    
	public AudioClip original;
	public bool trim;
	public UnityEngine.UI.Text debugText;
	public int sampleTime = 5;

	private AudioSource audioSource; 
	private string micName = null;
	private int sampleRate = 11025 ;
	private int counter = 0;

	private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
	private readonly IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library
	
	void Start() {
		//  StoreAudioFileFingerprintsInStorageForLaterRetrieval(Path.Combine(Application.streamingAssetsPath, "sweden.wav"));
		StoreAudioClipFingerprintsInStorageForLaterRetrieval(original);

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


	public void StoreAudioClipFingerprintsInStorageForLaterRetrieval(AudioClip clip)
	{
		var track = new TrackData("unknown", "ARTIST", "TRACK", "ALBUM", 1988, 290);
		var audioSamples = AudioClipBridge.AudioClipToAudioSamples(clip);

//Debug.Log("Storing reference file (samples: " + audioSamples.Samples.Length);
		// store track metadata in the datasource
		var trackReference = modelService.InsertTrack(track);

		// create hashed fingerprints
		var hashedFingerprints = FingerprintCommandBuilder.Instance
			.BuildFingerprintCommand()
			.From(audioSamples)
			.UsingServices(audioService)
			.Hash()
			.Result;
								
		// store hashes in the database for later retrieval
		modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
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
		string text = debugText.text;
		text = Regex.Replace(text, @"\((\d+)\)", "("+ ++counter +")");
		debugText.text = text;

		audioSource.clip = Microphone.Start (micName, false, 999, this.sampleRate);
		Invoke("StopRecord", sampleTime);

		return Microphone.IsRecording (micName);
	}


	public void StopRecord()
	{
		if (Microphone.IsRecording (micName)) {
			Microphone.End (micName);
		}

		AudioClip clip;
		if(trim) {
			clip = SavWav.TrimSilence(audioSource.clip, trimCutoff);
		} 
		else 
		{	clip = audioSource.clip;

		}
		if(clip == null)
		{
			print("Clip trimmed to 0");
			return;
		}
		//SavWav.Save(DateTime.Now.ToString("yyyyMMddHHmmss"), clip);

		ResultEntry match = GetBestMatchForAudioClip(clip);
		
		if(match != null && debugText != null){
			string text = DateTime.Now.ToString("HH:mm:ss") + ": found track! (c: " + match.Confidence.ToString("F3") + "; s:" + match.QueryMatchStartsAt.ToString("F3") + ")\n" + debugText.text;
			debugText.text = text;
			//Debug.Log("Found track " + match.Track.Title + " with confidence: " + match.Confidence + "; starts at:" + match.QueryMatchStartsAt);
		}
		else { 
			//Debug.Log("Found NO match");
		}
        Destroy(clip);

		StartRecord();
	}
}
