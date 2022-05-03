using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace AudioRecording
{
    class AudioRecorder
    {
        MediaCapture mediaCapture = new MediaCapture();
        //StorageFolder folder = KnownFolders.DocumentsLibrary;
        LowLagMediaRecording _mediaRecording;
        bool isRecording = false;

        public async void Init()
        {
            await InitMediaCapture();

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
                UI.WindowEnd();

            })) ;
            SK.Shutdown();
        }

        void ToggleRecording()
        {
            if (!isRecording)
            {
                StartRecording();
                isRecording = true;
            }
            else
            {
                StopRecording();
                isRecording = false;
            }

        }

        async Task InitMediaCapture()
        {
            var allAudioDevices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
            DeviceInformation microphone = allAudioDevices.FirstOrDefault();

            MediaCaptureInitializationSettings mediaInitSettings = new MediaCaptureInitializationSettings
            {
                AudioDeviceId = microphone.Id,
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };

            await mediaCapture.InitializeAsync(mediaInitSettings);

            mediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync("audio.mp3", CreationCollisionOption.GenerateUniqueName);
            _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High), file);

            // e.g. C:\Users\cwule\AppData\Local\Packages\3c8d17f6-227f-4982-a0ca-44b8dd8e9be1_qaswtywkfkn9m
            string appPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }


        async void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
        {
            await _mediaRecording.StopAsync();
            System.Diagnostics.Debug.WriteLine("Record limitation exceeded.");
        }

        async void StartRecording()
        {
            await _mediaRecording.StartAsync();
        }

        async void StopRecording()
        {
            await _mediaRecording.StopAsync();
        }

    }
}
