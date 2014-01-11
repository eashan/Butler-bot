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
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Speech.Synthesis;
using System.IO;


namespace KinectSpeechRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        KinectSensor myKinect;
        RecognizerInfo kinectRecognizerInfo;
        SpeechRecognitionEngine recognizer;
        SpeechSynthesizer speechoutput;

        KinectAudioSource kinectSource;

        Stream audioStream;

        private RecognizerInfo findKinectRecognizerInfo()
        {
            var recognizers = SpeechRecognitionEngine.InstalledRecognizers();

            foreach (RecognizerInfo recInfo in recognizers)
            {
                // look at each recognizer info value to find the one that works for Kinect
                if (recInfo.AdditionalInfo.ContainsKey("Kinect"))
                {
                    string details = recInfo.AdditionalInfo["Kinect"];
                    if (details == "True" && recInfo.Culture.Name == "en-US")
                    {
                        // If we get here we have found the info we want to use
                        return recInfo;
                    }
                }
            }
            return null;
        }


        private void createSpeechEngine()
        {
            kinectRecognizerInfo = findKinectRecognizerInfo();

            if (kinectRecognizerInfo == null)
            {
                MessageBox.Show("Kinect recognizer not found", "Kinect Speech Demo");
                Application.Current.Shutdown();
                return;
            }

            try
            {
                recognizer = new SpeechRecognitionEngine(kinectRecognizerInfo);
            }
            catch
            {
                MessageBox.Show("Speech recognition engine could not be loaded", "Kinect Speech Demo");
                Application.Current.Shutdown();
            }
        }

        private void buildCommands()
        {
            Choices commands = new Choices();

            commands.Add("who are you?");
            commands.Add("what is your name?");
            commands.Add("can i have a drink?");
            commands.Add("what is your purpose?");
            commands.Add("who is your master?");
            commands.Add("dad is here");
            commands.Add("who is dumbass");


            GrammarBuilder grammarBuilder = new GrammarBuilder();

            grammarBuilder.Culture = kinectRecognizerInfo.Culture;
            grammarBuilder.Append(commands);

            Grammar grammar = new Grammar(grammarBuilder);

            recognizer.LoadGrammar(grammar);
        }

        private void setupAudio()
        {
            try
            {
                myKinect = KinectSensor.KinectSensors[0];
                myKinect.Start();

                kinectSource = myKinect.AudioSource;
                kinectSource.BeamAngleMode = BeamAngleMode.Adaptive;
                audioStream = kinectSource.Start();
                recognizer.SetInputToAudioStream(audioStream, new SpeechAudioFormatInfo(
                                                      EncodingFormat.Pcm, 16000, 16, 1,
                                                      32000, 2, null));
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                MessageBox.Show("Audio stream could not be connected","Kinect Speech Demo");
                Application.Current.Shutdown();
            }
        }

        private void SetupSpeechRecognition()
        {
            createSpeechEngine();

            buildCommands();

            setupAudio();

            recognizer.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
        }

        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.8f)
            {
               wordTextBlock.Text=e.Result.Text;
                if(wordTextBlock.Text=="can i have a drink?")
                    speechoutput.Speak("please have it");
                if (wordTextBlock.Text == "what is your name?")
                    speechoutput.Speak("my name is Alfred");
                if (wordTextBlock.Text == "who are you?")
                    speechoutput.Speak("I am A Butler named Alfred");
                if (wordTextBlock.Text == "what is your purpose?")
                    speechoutput.Speak("I am designed to obey my master!");
                if (wordTextBlock.Text == "dad is here")
                    speechoutput.Speak("hello dad, how are you?");
                if (wordTextBlock.Text == "who is your master?")
                    speechoutput.Speak("my master is Abhishek Sawarkar");
                if (wordTextBlock.Text == "who is dumbass")
                    speechoutput.Speak("My friend Shubhankar is a dumbass!");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            speechoutput = new SpeechSynthesizer();
            speechoutput.Speak("Ready to go!");
            SetupSpeechRecognition();

        }
    }
}
