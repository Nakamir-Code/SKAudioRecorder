using StereoKit;
using System;

namespace AudioRecording
{
    class Program
    {   
        static void Main(string[] args)
        {
            AudioRecorder recorder = new AudioRecorder();
            recorder.Init();
        }

     
    }
}
