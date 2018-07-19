

This is a test suite linking `soundfingerprinting` (https://github.com/AddictedCS/soundfingerprinting) c# library to a Unity3D project.



How to run?
===========

* Open in Unity
* Ensure a microphone and speakers are present and working
* Play `Assets/StreamingAssets/sweden_5s.wav` in a media player on repeat
* Run the Unity project in the Unity Editor
* Every 3 seconds it will attempt to match the sound it hears through the microphone with the stored sample (see Console)

NOTE:

Make sure the sample has the following import settings:

- Load type: `Decompress on Load`
- Preload Audio Data: true
- Compression Format: `PCM`

