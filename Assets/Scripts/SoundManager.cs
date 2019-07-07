using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [Serializable]
    public class AudioCtrl
    {
        // hold values that correlate to the basePlayTimes array
        private float[] pitchRanges;

        // play times for lowSource - tempo/rhythm for the other sounds to follow
        public float[] playTimes;
        // play length for lowSource
        public float playTimer;

        // amount of the time between sound playing
        public float interval = 0f;
        // timing interval or wait period between sound plays
        public float intervalTimer = 0f;

        // the number of times the sound will play within a measure
        // it is lenght of the basePlayTimes and basePitchRanges arrays
        public int cordCount;
        // tracks of which pitch value we need to attach to the sound
        public int rangeCount = 0;

        public AudioCtrl()
        {
            playTimer = 0f;
            interval = 0f;
            intervalTimer = 0f;
            cordCount = 0;
            rangeCount = 0;
        }

        /// <summary>
        /// calculates how the sound will be played sounds within measure
        /// </summary>
        public void CalculateAudio(float measure, int minFreq, int maxFreq, float low, float high)
        {
            float playTotal = 0f;

            float lastPitch = Random.Range(low, high); // current pitch value
            int switchPitchCount = Random.Range(3, maxFreq); // number of the times the pitch value changes
            int switchPitch = 0; // flag to indicate whether pitch should increase or decrease
            int pitchDir = Random.Range(0, 2); // 1 for increasing, 0 for decreasing

            cordCount = Random.Range(minFreq, maxFreq);
            playTimes = new float[cordCount];
            pitchRanges = new float[cordCount];

            for (int i = 0; i < cordCount; i++)
            {
                playTimes[i] = Random.Range(minFreq / cordCount, measure / cordCount);
                playTotal += playTimes[i];
                //pitchRanges[i] = Random.Range(low, high);
                if (pitchDir == 0)
                    lastPitch = pitchRanges[i] = Random.Range(low, lastPitch);
                else if (pitchDir == 1)
                    lastPitch = pitchRanges[i] = Random.Range(lastPitch, high);

                switchPitch++;
                if(switchPitch == switchPitchCount)
                {
                    if (pitchDir == 0)
                        pitchDir = 1;
                    else
                        pitchDir = 0;
                }
                playTimer = playTimes[0];

                interval = (measure - playTotal) / cordCount;
                intervalTimer = interval;
            }

            playTimer = playTimes[0];
            interval = (measure - playTotal) / cordCount;
            intervalTimer = interval;
        }

        // ref means their values will be globally updated in the script
        public void PlaySoundLine(AudioSource audio)
        {
            if (rangeCount >= cordCount)
                rangeCount = 0;

            if (playTimer > 0)
            {
                playTimer -= Time.deltaTime;
                if (!audio.isPlaying)
                {
                    audio.pitch = pitchRanges[rangeCount];
                    audio.Play();
                    rangeCount++;
                }
            }

            else if (playTimer <= 0)
            {
                audio.Stop();

                if (intervalTimer > 0)
                {
                    intervalTimer -= Time.deltaTime;
                }
                else if (intervalTimer <= 0)
                {
                    playTimer = playTimes[rangeCount];
                    intervalTimer = interval;
                }
            }
        }

    }


    public static SoundManager instance = null;

    public AudioSource highSource;
    public AudioSource midSource;
    public AudioSource lowSource;

    public float lowPitchRange = 0f;
    public float highPitchRange = 0f;

    // the time frame in which all the sounds are played
    public float measure;

    public AudioCtrl baseAudio;
    public AudioCtrl midAudio;
    public AudioCtrl highAudio;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        lowPitchRange = .25f;
        highPitchRange = 1.75f;

        baseAudio = new AudioCtrl();
        midAudio = new AudioCtrl();
        highAudio = new AudioCtrl();

        FormAudio(false);
    }

    void Update()
    {
        baseAudio.PlaySoundLine(lowSource);
        midAudio.PlaySoundLine(midSource);
        highAudio.PlaySoundLine(highSource);
    }

    public void FormAudio(bool tension)
    {
        if (tension)
            measure = Random.Range(1f, 3f); // increase the frequency (tempo)
        else
            measure = Random.Range(10f, 20f);

        baseAudio.CalculateAudio(measure, 3, 7, lowPitchRange, highPitchRange);
        midAudio.CalculateAudio(measure, 2, 6, lowPitchRange, highPitchRange);
        highAudio.CalculateAudio(measure, 5, 10, lowPitchRange, highPitchRange);
    }

}
