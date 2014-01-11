using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;

namespace KinectSkeletonViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor myKinect;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Check to see if a Kinect is available
            if (  KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No Kinects detected", "Camera Viewer");
                Application.Current.Shutdown();
            }

            // Get the first Kinect on the computer
            myKinect = KinectSensor.KinectSensors[0];

            // Start the Kinect running and select the depth camera
            try
            {
                myKinect.SkeletonStream.Enable();
                myKinect.Start();
            }
            catch
            {
                MessageBox.Show("Kinect initialise failed", "Camera Viewer");
                Application.Current.Shutdown();
            }

            // connect a handler to the event that fires when new frames are available

            myKinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(myKinect_SkeletonFrameReady);

        }

        Brush skeletonBrush = new SolidColorBrush(Colors.Red);

        void addLine(Joint j1, Joint j2)
        {
            Line boneLine = new Line();
            boneLine.Stroke = skeletonBrush;
            boneLine.StrokeThickness = 5;

            ColorImagePoint j1P = myKinect.MapSkeletonPointToColor(j1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            boneLine.X1 = j1P.X;
            boneLine.Y1 = j1P.Y;

            ColorImagePoint j2P = myKinect.MapSkeletonPointToColor(j2.Position, ColorImageFormat.RgbResolution640x480Fps30);
            boneLine.X2 = j2P.X;
            boneLine.Y2 = j2P.Y;

            skeletonCanvas.Children.Add(boneLine);
        }

    

        void myKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            string message = "No Skeleton Data";
            string qualityMessage = "";

            // Remove the old skeleton
            skeletonCanvas.Children.Clear();
            Brush brush = new SolidColorBrush(Colors.Red);

            float closestdis = 1000f;
            int closestID = 0;

            Skeleton[] skeletons = null;
           

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                }
            }

            if (skeletons == null) return;


            foreach (Skeleton skeleton in skeletons)
            {
                if (SkeletonTrackingState.Tracked==skeleton.TrackingState)
                {
                    if (skeleton.Position.Z > closestdis)
                    {
                        closestID = skeleton.TrackingId;
                        closestdis = skeleton.Position.Z;
                    }

                    if (closestID > 0)
                    {
                        myKinect.SkeletonStream.ChooseSkeletons(closestID);
                    }

                    Joint headJoint = skeleton.Joints[JointType.Head];
                    Joint hipCenter = skeleton.Joints[JointType.HipCenter];

                    if (headJoint.TrackingState != JointTrackingState.NotTracked)
                    {
                        if (headJoint.TrackingState == JointTrackingState.Inferred)
                        {
                            message = message + " I";
                        }
                    }

                    if (skeleton.ClippedEdges == 0)
                    {
                        qualityMessage = "Good Quality";
                    }
                    else
                    {
                        if ((skeleton.ClippedEdges & FrameEdges.Bottom) != 0)
                            qualityMessage += "Move up ";
                        if ((skeleton.ClippedEdges & FrameEdges.Top) != 0)
                            qualityMessage += "Move down ";
                        if ((skeleton.ClippedEdges & FrameEdges.Right) != 0)
                            qualityMessage += "Move left ";
                        if ((skeleton.ClippedEdges & FrameEdges.Left) != 0)
                            qualityMessage += "Move right ";
                    }

                    ColorImagePoint headPoint = myKinect.MapSkeletonPointToColor(headJoint.Position, ColorImageFormat.RgbResolution640x480Fps30);
                    message = string.Format("Head: X:{0:000} Y:{1:000}",
                        headPoint.X,
                        headPoint.Y);

                    if (headJoint.TrackingState == JointTrackingState.Inferred)
                    {
                        message = message + " I";
                    }

                    // Spine
                    addLine(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
                    addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);

                    // Left leg
                    addLine(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
                    addLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
                    addLine(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
                    addLine(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
                    addLine(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

                    // Right leg
                    addLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);
                    addLine(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
                    addLine(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
                    addLine(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);

                    // Left arm
                    addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
                    addLine(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
                    addLine(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
                    addLine(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

                    // Right arm
                    addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
                    addLine(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
                    addLine(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
                    addLine(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);
                }
            }

            StatusTextBlock.Text = message;
            QualityTextBlock.Text = qualityMessage;
        }
    }
}
