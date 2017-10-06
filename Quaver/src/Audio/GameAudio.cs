﻿using System;
using System.IO;
using ManagedBass;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Audio
{
    internal class GameAudio
    {
        public int Stream { get; set; }

        /// <summary>
        /// Constructor - We check if the file path is correct, and if it is, we try to load the audio stream
        /// </summary>
        /// <param name="filePath"></param>
        public GameAudio(string filePath)
        {
            // If the file doesn't exist, the stream is set to 0, which means it cannot be played.
            if (!File.Exists(filePath))
            {
                Stream = 0;
                return;
            }

            // Otherwise, we are going to load the audio stream
            LoadAudioStream(filePath);
        }

        /// <summary>
        ///     Loads an audio stream, called automatically upon instantiation
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadAudioStream(string filePath)
        {
            if (Bass.Init())
            {
                var stream = Bass.CreateStream(filePath);

                if (stream != 0)
                    Stream = stream;

                // Free the stream when the playback ends
                Bass.ChannelAddFlag(Stream, BassFlags.AutoFree);
                return;
            }

            Stream = 0;
            Console.WriteLine("[AUDIO ENGINE] Error: {0}", Bass.LastError);
        }

        /// <summary>
        /// Plays the current audio stream at a given preview time if specified.
        /// </summary>
        internal void Play(double previewTime = 0)
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Stopped)
                return;

            // Set the volume of the track, to that of what is in the config.
            Bass.ChannelSetAttribute(Stream, ChannelAttribute.Volume, (float) Configuration.VolumeGlobal / 100);

            // Set the position to play the song at 
            Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, previewTime / 1000));

            // Start playing!
            Bass.ChannelPlay(Stream);
            Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {Stream} has started playing at position: {previewTime}");
        }

        /// <summary>
        ///     Pauses the current audio stream
        /// </summary>
        internal void Pause()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Playing)
                return;

            Bass.ChannelPause(Stream);
            Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {Stream} has been paused.");
        }

        /// <summary>
        ///     Resumes the current audio stream if it is paused.
        /// </summary>
        internal void Resume()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Paused)
                return;

            Bass.ChannelPlay(Stream);
            Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {Stream} has been resumed.");
        }

        /// <summary>
        ///     Completely stops the current audio stream and frees any resources it took.
        /// </summary>
        internal void Stop()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Stopped)
                return;

            // Completely stop the stream and free its resources
            Bass.ChannelStop(Stream);
            Bass.StreamFree(Stream);
        }

        /// <summary>
        ///     Gets the current audio position of the stream in milliseconds.
        ///     Returns 0.0 if the stream is 0
        /// </summary>
        /// <returns></returns>
        internal double GetAudioPosition()
        {
            return (Stream == 0) ? 0.0f : Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetPosition(Stream));
        }
    }
}