using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioRecording
{
    class App
    {

        public void Init()
        {
            SKSettings settings = new SKSettings
            {
                appName = "AudioRecording",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            AudioRecorder recorder = SK.AddStepper<AudioRecorder>();

            while (SK.Step(() =>
            {

            }));

            SK.RemoveStepper<AudioRecorder>();
            SK.Shutdown();
        }
    }
}
