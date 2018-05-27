

Trying to figure out how to link `soundfingerprinting` (https://github.com/AddictedCS/soundfingerprinting) c# library to Unity.



How to run?
===========

* Open in Unity
* Ensure a microphone and speakers are present and working
* Play `Assets/StreamingAssets/sweden_5s.wav` in a media player on repeat
* Run the Unity project in the Unity Editor
* Every 3 seconds it will attempt to match the sound it hears through the microphone with the stored sample


Note that it works if you call `GetBestMatchForFile` instead of `GetBestMatchForAudioClip` but obviously I would like to see it working without file IO. I would like `AudioClipBridge.AudioClipToAudioSamples` to return the `AudioSamples` correctly..