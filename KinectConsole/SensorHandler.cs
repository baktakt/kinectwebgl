using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using Microsoft.Kinect.Toolkit.Controls;
using Newtonsoft.Json;

namespace KinectConsole
{
    public class SensorHandler
    {
        private KinectSensorChooser sensorChooser;
        private static InteractionStream interactionStream;
        static bool _initialized = false;
        private bool lightsOn = false;
        private Skeleton[] skeletonData;

        public SensorHandler()
        {
            ConnectSensor();
        }

        private void ConnectSensor()
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooser.Start();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            Console.WriteLine("Connecting sensor.");   

            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();
                    this.skeletonData = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    args.NewSensor.SkeletonFrameReady += NewSensor_SkeletonFrameReady;
                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        error = true;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (!error)
            {
                Console.WriteLine("Sensor connected successfully. Starting to track.");   
            }
        }

        void NewSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null && this.skeletonData != null)
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                    DataCaptured();
                }
            }
        }

        private void DataCaptured()
        {
            Skeleton skeleton = this.skeletonData.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked).FirstOrDefault();
            if (skeleton != null)
            {
                var skeletonJson = JsonConvert.SerializeObject(skeleton);
                Console.WriteLine(skeletonJson);
                /*if (_sockets.Count > 0)
                {
                    foreach (var socket in _sockets)
                    {
                        socket.Send(jsonObject);
                    }
                }*/
            }
        }
    }
}
