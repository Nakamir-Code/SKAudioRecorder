using StereoKit;
using System;

namespace AudioRecording
{

    // Basic Microphone recording sample using Stereokit
    class AudioRecorder
    {
        float[] micBuffer = new float[0];
        Sound micStream;

        public void Init()
        {
            InitMic();

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
            micStream = Sound.CreateStream(10f);
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

                    int samples = Microphone.Sound.ReadSamples(ref micBuffer);

                    micStream.WriteSamples(micBuffer);
                }

                
            })) ;
            SK.Shutdown();
        }

        void ToggleRecording()
        {
            if (!Microphone.IsRecording)
            {
                StartRecording();                
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
        }

        void PlayRecording()
        {
            micStream.Play(Input.Head.position);
        }
    }
}
