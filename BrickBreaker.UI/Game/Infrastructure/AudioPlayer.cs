using BrickBreaker.Models;                  // Imports models from BrickBreaker
using BrickBreaker.UI.Game.Infrastructure;  // Imports infrastructure (interfaces, etc.) from the game
using NAudio.Wave;                          // Imports the NAudio library for audio playback
using System;                               // Imports basic .NET functionality (e.g., IDisposable)

public sealed class NaudioGameAudio : IGameAudio, IDisposable // Defines a sealed class implementing game audio and IDisposable pattern
{
    private IWavePlayer? soundtrackPlayer;      // Handles audio playback (nullable)
    private AudioFileReader? soundtrackReader;  // Reads audio files (nullable)
    private bool musicActive = false;           // Flag that tracks if music should play
    private EventHandler<StoppedEventArgs>? playbackStoppedHandler; // Event handler for when a track stops playing
    private readonly string[] playlist = new string[] // Playlist with paths to music files
    {
        "Assets/Sounds/Backbeat.mp3",
        "Assets/Sounds/Arpent.mp3",
        "Assets/Sounds/findingnemo.mp3"
    };
    private int currentTrack = 0;               // Tracks the current track in the playlist

    public void Pause()                         // Function to pause and resume music
    {
        if (soundtrackPlayer != null)           // If the audio player exists
        {
            if (soundtrackPlayer.PlaybackState == PlaybackState.Playing)   // If it's currently playing
                soundtrackPlayer.Pause();       // Pause music
            else if (soundtrackPlayer.PlaybackState == PlaybackState.Paused) // If it's paused
                soundtrackPlayer.Play();        // Resume music
        }
    }

    public void Next()                          // Function to jump to the next track
    {
        if (soundtrackPlayer != null)           // If the audio player exists
        {
            soundtrackPlayer.Stop();            // Stop current track (triggers playbackStopped event)
        }
    }

    public void StartMusic()                    // Starts playing music
    {
        musicActive = true;                     // Set flag that music is active
        soundtrackReader = new AudioFileReader(playlist[currentTrack]);     // Load current audio file
        soundtrackPlayer = new WaveOutEvent();                             // Create a new audio player

        playbackStoppedHandler = (s, e) =>      // When a track stops playing
        {
            if (!musicActive) return;           // If music is not active, do nothing
            currentTrack = (currentTrack + 1) % playlist.Length;           // Move to the next track (loops the playlist)
            soundtrackReader?.Dispose();        // Dispose previous file reader
            soundtrackReader = new AudioFileReader(playlist[currentTrack]); // Load the next track
            soundtrackPlayer?.Init(soundtrackReader);                      // Initialize the audio player
            soundtrackPlayer?.Play();           // Start playing the next track
        };

        soundtrackPlayer.PlaybackStopped += playbackStoppedHandler;         // Attach the event handler
        soundtrackPlayer.Init(soundtrackReader);                            // Initialize the first audio file
        soundtrackPlayer.Play();                                            // Start playback
    }

    public void StopMusic()                     // Stops all music
    {
        musicActive = false;                    // Set music as inactive

        if (soundtrackPlayer != null)           // If the audio player exists
        {
            if (playbackStoppedHandler != null) // If the event handler exists
                soundtrackPlayer.PlaybackStopped -= playbackStoppedHandler; // Detach event handler

            soundtrackPlayer.Stop();            // Stop the audio player
            soundtrackPlayer.Dispose();         // Dispose audio player resources
            soundtrackPlayer = null;            // Nullify the reference
        }

        if (soundtrackReader != null)           // If the audio file reader exists
        {
            soundtrackReader.Dispose();         // Dispose resources
            soundtrackReader = null;            // Nullify the reference
        }
    }

    public void Dispose()                       // Release/cleanup method as per IDisposable pattern
    {
        StopMusic();                            // Stop and clean up music resources
        GC.SuppressFinalize(this);              // Tell Garbage Collector to skip the finalizer
    }
}
