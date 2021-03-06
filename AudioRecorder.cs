using StereoKit;
using StereoKit.Framework;
using System;
using System.Diagnostics;

namespace AudioRecording
{
    class AudioRecorder : IStepper
    {
        // the microphone buffer, will be refilled every frame
        float[] micBuffer = new float[0];
        // the sound stream, will be created in the end from the accumulated micbuffer
        Sound micStream;

        // the array where the micbuffer will be copied each frame
        float[] totalAudio;

        // keep track where to write in the totalAudio array
        int soundPos;

        // the recording Start time
        float startTime;

        // IStepper needs this
        public bool Enabled => true;

        // IStepper needs this
        public bool Initialize() => true;

        public void Step()
        {

            Vec3 buttonLoc = Input.Head.position + Input.Head.Forward * 50 * U.cm;
            Pose buttonPose = new Pose(buttonLoc, Quat.LookDir(-Input.Head.Forward));

            UI.WindowBegin("Window Button", ref buttonPose);
            if (UI.Button("Toggle Recording"))
                ToggleRecording();
            if (!Microphone.IsRecording && micStream != null && UI.Button("Play Recording"))
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
                // if totalAudio[] is full/ maximum recording time exceeded, stop recording
                else
                    StopRecording();
            }
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
            // the maximum length of the recording is 48000(bitrate) * x seconds
            totalAudio = new float[48000 * 60];
            soundPos = 0;
            Microphone.Start();
            startTime = Time.Totalf;
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

        public void Shutdown()
        {
            Microphone.Stop();
        }
    }
}
