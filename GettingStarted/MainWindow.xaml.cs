using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Fleck;
using Microsoft.Kinect.Toolkit.Interaction;
using Newtonsoft.Json;

namespace GettingStarted
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensorChooser sensorChooser;
        private static InteractionStream interactionStream;
        private static List<IWebSocketConnection> _sockets;
        static bool _initialized = false;
        private bool lightsOn = false;
        private Skeleton[] skeletonData;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSockets();
            Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {

            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
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
                //KinectRegion.AddHandPointerMoveHandler(kinectRegion, OnHandleHandMove);
                //KinectRegion.AddHandPointerGripHandler(kinectRegion, OnHandleGrip);
                
                kinectRegion.KinectSensor = args.NewSensor;


                //interactionStream = new InteractionStream(args.NewSensor, new IInteractionClient());
                //interactionStream.InteractionFrameReady += new EventHandler<InteractionFrameReadyEventArgs>(interactionstream_InteractionFrameReady);
            }
        }

        void TrackSkeletonData()
        { 
        
        }

        void NewSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) 
            {
                if (skeletonFrame != null && this.skeletonData != null) 
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                    /*
                    if (!this.sensorChooser.Kinect.SkeletonStream.AppChoosesSkeletons) 
                    {
                        this.sensorChooser.Kinect.SkeletonStream.AppChoosesSkeletons = true;
                    }
                    float closestDistance = 10000f;
                    int closestId = 0;

                    if (this.sensorChooser.Kinect.SkeletonStream != null)
                    {
                        try
                        {
                            foreach (Skeleton skeleton in this.skeletonData.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                            {
                                if (skeleton.Position.Z < closestDistance)
                                {
                                    closestId = skeleton.TrackingId;
                                }
                            }
                        }
                        catch { }
                        
                    }

                    if (closestId > 0) 
                    {
                        this.sensorChooser.Kinect.SkeletonStream.ChooseSkeletons(closestId);
                    }
                    */
                    DataCaptured();
                }
            }
        }

        private void DataCaptured()
        {
            Skeleton skeleton = this.skeletonData.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked).FirstOrDefault();
            if (skeleton != null) 
            {
                var rightHandJoint = skeleton.Joints.Where(j => j.JointType == JointType.HandRight).FirstOrDefault();
                var leftHandJoint = skeleton.Joints.Where(j => j.JointType == JointType.HandLeft).FirstOrDefault();
                var headJoint = skeleton.Joints.Where(j => j.JointType == JointType.Head).FirstOrDefault();
                int multiplier = 100;

                UserModel trackedUser = new UserModel
                {
                    RightHandJointPosition = new Position3D
                    {
                        X = (float)(rightHandJoint.Position.X),
                        Y = (float)(rightHandJoint.Position.Y),
                        Z = (float)(rightHandJoint.Position.Z)
                    },

                    LeftHandJointPosition = new Position3D
                    {
                        X = (float)(leftHandJoint.Position.X),
                        Y = (float)(leftHandJoint.Position.Y),
                        Z = (float)(leftHandJoint.Position.Z)
                    },

                    HeadJointPosition = new Position3D 
                    {
                        X = (float)(headJoint.Position.X),
                        Y = (float)(headJoint.Position.Y),
                        Z = (float)(headJoint.Position.Z)
                    }
                };

                var jsonObject = JsonConvert.SerializeObject(trackedUser);
              
                if(_sockets.Count > 0)
                {
                    foreach (var socket in _sockets)
                    {
                        socket.Send(jsonObject);
                    }
                }

                statusLabel.Content = jsonObject;
            }
            
        }
        
        private static void InitializeSockets()
        {
            _sockets = new List<IWebSocketConnection>();

            var server = new WebSocketServer("ws://10.1.0.165:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Connected to " + socket.ConnectionInfo.ClientIpAddress);
                    _sockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Disconnected from " + socket.ConnectionInfo.ClientIpAddress);
                    _sockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                };
            });

            _initialized = true;

            Console.ReadLine();
        }

        private void OnHandleHandMove(object source, HandPointerEventArgs args)
        {
            
            string json;

            if (!args.HandPointer.IsInGripInteraction)
            {
            }
            else
            {
                if (lightsOn)
                    lightsOn = false;
                else
                    lightsOn = true;
            }
            json = "{\"x\":" + (int)args.HandPointer.GetPosition(kinectRegion).X + ",\"y\":" + (int)args.HandPointer.GetPosition(kinectRegion).Y + ", \"lightsOn\":" + lightsOn.ToString().ToLower() + "}";
            foreach (var socket in _sockets)
            {
                socket.Send(json);
            }

            statusLabel.Content = json;
        }

        private void OnHandleGrip(object sender, HandPointerEventArgs args)
        {
            statusLabel.Content = "X=" + args.HandPointer.GetPosition(kinectRegion).X + "Y=" + args.HandPointer.GetPosition(kinectRegion).Y;
            string json = "{\"x\":" + (int)args.HandPointer.GetPosition(kinectRegion).X + ",\"y\":" + (int)args.HandPointer.GetPosition(kinectRegion).Y + ", \"isGrabbing\":" + args.HandPointer.IsInGripInteraction.ToString().ToLower() + "}";
            foreach (var socket in _sockets)
            {
                socket.Send(json);
            }
        }

        private void KinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            
        }

        private void ButtonOnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Well done!");
        }
    }
}
