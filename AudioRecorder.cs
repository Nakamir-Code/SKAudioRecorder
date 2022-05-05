using StereoKit;
using System;
using System.Diagnostics;

namespace AudioRecording
{
    class AudioRecorder
    {
        // the microphone buffer, will be refilled every frame
        float[] micBuffer = new float[0];
        // the sound stream, will be created in the end from the accumulated micbuffer
        Sound micStream;

        // the array where the micbuffer will be copied each frame, the maximum length of the recording is 48000 (bitrate) * x seconds
        float[] totalAudio = new float[48000 * 60];

        // keep track where to write in the totalAudio array
        int soundPos = 0;

        // the recording Start time
        float startTime;

        public void Init()
        {
            //InitMic();

            InitSK();

            SKStep();
        }

        void InitSK()
        {
            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "AudioRecording",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);
        }

        void InitMic()
        {
            // Not used anymore
        }

        void SKStep()
        {
            // Core application loop
            while (SK.Step(() =>
            {
                Vec3 buttonLoc = Input.Head.position + Input.Head.Forward * 50 * U.cm;
                Pose buttonPose = new Pose(buttonLoc, Quat.LookDir(-Input.Head.Forward));

                UI.WindowBegin("Window Button", ref buttonPose);
                if (UI.Button("Toggle Recording"))
                    ToggleRecording();
                if (UI.Button("Play Recording"))
                    PlayRecording();
                UI.WindowEnd();


                if (Microphone.IsRecording)
                {
                    micBuffer = new float[Microphone.Sound.UnreadSamples];

                    // Debug.WriteLine("Unread samples: " + Microphone.Sound.UnreadSamples.ToString());

                    // read all samples acquired during last frame to micBuffer
                    int samples = Microphone.Sound.ReadSamples(ref micBuffer);
                    // Debug.WriteLine("Samples this frame: " + samples.ToString());

                    // save micBuffer to totalAudio recording Array
                    if (soundPos + samples < totalAudio.Length)
                    {
                        for (int i = 0; i < samples; i++)
                            totalAudio[soundPos + i] = micBuffer[i];
                        soundPos = soundPos + samples;
                    }
                    // if totalAudio is full/ maximum recording time exceeded, stop recording
                    else
                        StopRecording();
                }
            })) ;
            SK.Shutdown();
        }

        void ToggleRecording()
        {
            if (!Microphone.IsRecording)
            {
                StartRecording();
                startTime = Time.Totalf;
            }
            else
            {
                StopRecording();
            }
        }


        void StartRecording()
        {
            Microphone.Start();
        }

        void StopRecording()
        {
            Microphone.Stop();

            // cut the totalAudio array to the actual length of the recording
            Array.Resize(ref totalAudio, soundPos);

            float recordingTime = Time.Totalf - startTime;
            //micStream = Sound.CreateStream(totalAudio.Length / 48000f);

            // create a StereoKit.Sound stream with the length of the actual recording
            micStream = Sound.CreateStream(recordingTime);
            micStream.WriteSamples(totalAudio);
        }

        void PlayRecording()
        {
            micStream.Play(Input.Head.position);
        }
    }
}
