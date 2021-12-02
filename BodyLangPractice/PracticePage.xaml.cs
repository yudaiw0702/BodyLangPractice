using BodyLangPractice.BodyLangModelPage;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BodyLangPractice
{
    /// <summary>
    /// PracticePage.xaml の相互作用ロジック
    /// </summary>
    public partial class PracticePage : Page
    {

        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 30;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// List of colors for each body tracked
        /// </summary>
        private List<Pen> bodyColors;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        // Kinect (MultiFrame)
        private KinectSensor kinect;
        private MultiSourceFrameReader multiFrameReader;

        // Gesture Builder
        private VisualGestureBuilderDatabase gestureDatabase;
        private VisualGestureBuilderFrameSource gestureFrameSource;
        private VisualGestureBuilderFrameReader gestureFrameReader;

        //Gesture
        private Gesture ohayo;
        private Gesture omedeto;
        private Gesture yasumu;
        private Gesture wasureru;
        private Gesture sumu;
        private Gesture yamai;
        private Gesture benkyo;
        private Gesture tukuru;
        private Gesture konnitiha;
        private Gesture tanosii;
        private Gesture atumeru;
        private Gesture otukaresama;
        private Gesture neru;
        private Gesture keitaidenwa;
        private Gesture masuku;

        //Progress Gesture Frag
        private bool tanosii_flag;

        int index_next = 0;

        public int next_push_count;
        public static int correct_count;

        public PracticePage()
        {
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // populate body colors, one for each BodyIndex
            this.bodyColors = new List<Pen>();

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            Loaded += PracticePage_Loaded;

            _navi = frameModel.NavigationService;

            image.Visibility = Visibility.Hidden;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        void PracticePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }

            // Kinectへの接続
            try
            {
                kinect = KinectSensor.GetDefault();
                if (kinect == null)
                {
                    throw new Exception("Cannot open kinect v2 sensor.");
                }

                kinect.Open();

                // 初期設定
                Initialize();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

            var indexString = index_next + 1;
            textNumber.Text = indexString + " / " + uriList.Count;
        }


        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    int penIndex = 0;
                    foreach (Body body in this.bodies)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];

                        if (body.IsTracked)
                        {
                            this.DrawClippedEdges(body, dc);

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = InferredZPositionClamp;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }

                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        /// </summary>
        /// <param name="handState">state of the hand</param>
        /// <param name="handPosition">position of the hand</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }

        /// <summary>
        /// 初期設定
        /// </summary>
        private void Initialize()
        {

            // Gesturesの初期設定
            gestureDatabase = new VisualGestureBuilderDatabase(@"../../Gestures/gesture.gbd");
            gestureFrameSource = new VisualGestureBuilderFrameSource(kinect, 0);

            // 使用するジェスチャーをデータベースから取り出す
            foreach (var gesture in gestureDatabase.AvailableGestures)
            {
                if (gesture.Name == "ohayo") //おはよう
                { ohayo = gesture; }
                if (gesture.Name == "omedeto") //おめでとう
                { omedeto = gesture; }
                if (gesture.Name == "yasumu") //休む
                { yasumu = gesture; }
                if (gesture.Name == "wasureru") //忘れる
                { wasureru = gesture; }
                if (gesture.Name == "sumu") //住む
                { sumu = gesture; }
                if (gesture.Name == "yamai") //病気
                { yamai = gesture; }
                if (gesture.Name == "benkyo") //勉強
                { benkyo = gesture; }
                if (gesture.Name == "tukuru") //作る
                { tukuru = gesture; }
                if (gesture.Name == "konnitiha") //こんにちは
                { konnitiha = gesture; }
                if (gesture.Name == "tanosii") //楽しい
                { tanosii = gesture; }
                if (gesture.Name == "atumeru") //集める
                { atumeru = gesture; }
                if (gesture.Name == "otukaresama") //お疲れ様
                { otukaresama = gesture; }
                if (gesture.Name == "neru") //寝る
                { neru = gesture; }
                if (gesture.Name == "keitaidenwa") //携帯電話
                { keitaidenwa = gesture; }
                if (gesture.Name == "masuku") //マスク
                { masuku = gesture; }

                this.gestureFrameSource.AddGesture(gesture);
            }

            // ジェスチャーリーダーを開く
            gestureFrameReader = gestureFrameSource.OpenReader();
            gestureFrameReader.IsPaused = true;
            gestureFrameReader.FrameArrived += gestureFrameReader_FrameArrived;

            // フレームリーダーを開く (Color / Body)
            multiFrameReader = kinect.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            multiFrameReader.MultiSourceFrameArrived += multiFrameReader_MultiSourceFrameArrived;


        }

        private void multiFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiFrame = e.FrameReference.AcquireFrame();

            // Bodyを１つ探し、ジェスチャー取得の対象として設定
            if (!gestureFrameSource.IsTrackingIdValid)
            {
                using (BodyFrame bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        bodyFrame.GetAndRefreshBodyData(bodies);

                        foreach (var body in bodies)
                        {
                            if (body != null && body.IsTracked)
                            {
                                // ジェスチャー判定対象としてbodyを選択
                                gestureFrameSource.TrackingId = body.TrackingId;
                                // ジェスチャー判定開始
                                gestureFrameReader.IsPaused = false;
                            }
                        }
                    }
                }
            }
        }

        //gestureの判定時間管理
        int ohayo_time = 0, omedeto_time = 0, yasumu_time = 0, wasureru_time = 0, sumu_time = 0,
            yamai_time = 0, benkyo_time = 0, tukuru_time = 0, konnitiha_time = 0, tanosii_time = 0,
            atumeru_time = 0, otukaresama_time = 0, neru_time = 0, keitaidenwa_time = 0, masuku_time = 0;

        private void gestureFrameReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {

            using (var gestureFrame = e.FrameReference.AcquireFrame())
            {

                // ジェスチャーの判定結果がある場合
                if (gestureFrame != null && gestureFrame.DiscreteGestureResults != null)
                {
                    //Discrete : gesture
                    var result1 = gestureFrame.DiscreteGestureResults[ohayo];
                    var result2 = gestureFrame.DiscreteGestureResults[omedeto];
                    var result3 = gestureFrame.DiscreteGestureResults[yasumu];
                    //忘れるの手話：1付近でパーで上げる、0付近で下げてるグー
                    var result4 = gestureFrame.DiscreteGestureResults[wasureru];
                    var result5 = gestureFrame.DiscreteGestureResults[sumu];
                    var result6 = gestureFrame.DiscreteGestureResults[yamai];
                    var result7 = gestureFrame.DiscreteGestureResults[benkyo];
                    var result8 = gestureFrame.DiscreteGestureResults[tukuru];
                    var result9 = gestureFrame.DiscreteGestureResults[konnitiha];
                    //楽しいの手話：右手上で1付近、左手上で0付近
                    var result10 = gestureFrame.ContinuousGestureResults[tanosii];
                    var result11 = gestureFrame.DiscreteGestureResults[atumeru];
                    var result12 = gestureFrame.DiscreteGestureResults[otukaresama];
                    var result13 = gestureFrame.DiscreteGestureResults[neru];
                    var result14 = gestureFrame.DiscreteGestureResults[keitaidenwa];
                    var result15 = gestureFrame.DiscreteGestureResults[masuku];


                    if (index_next == 0)
                    {
                        if (result1.Confidence > 0.1)
                        {
                            textBlock1.Text = "おはよう：判定中";
                        }
                        if (result1.Confidence > 0.2)
                        {
                            textBlock1.Text = "おはよう：○";
                        }
                        textBlock2.Text = "おはよう：" + result1.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result1.Confidence >= 0.3)
                        {
                            sw_ohayo(true);
                            textBlock1.Text = "おはよう：◎";
                        }
                        else ohayo_time = 0;

                        if(ohayo_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result1.Confidence * 100);
                            Trace.WriteLine(dt + " おはよう：" + result + "%");
                        }
                    }

                    if (index_next == 1)
                    {
                        if (result1.Confidence > 0.1)
                        {
                            textBlock1.Text = "おめでとう：判定中";
                        }

                        if (result1.Confidence > 0.2)
                        {
                            textBlock1.Text = "おめでとう：○";
                        }

                        textBlock2.Text = "おめでとう：" + result2.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result2.Confidence >= 0.3)
                        {
                            sw_omedeto(true);
                            textBlock1.Text = "おめでとう：◎";
                        }
                        else omedeto_time = 0;

                        if (omedeto_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result2.Confidence * 100);
                            Trace.WriteLine(dt + " おめでとう：" + result + "%");
                        }
                    }

                    if (index_next == 2)
                    {
                        if (result3.Confidence > 0.1)
                        {
                            textBlock1.Text = "休む：判定中";
                        }

                        if (result3.Confidence > 0.2)
                        {
                            textBlock1.Text = "休む：○";
                        }
                        textBlock2.Text = "休む：" + result3.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result3.Confidence >= 0.3)
                        {
                            sw_yasumu(true);
                            textBlock1.Text = "休む：◎";
                        }
                        else yasumu_time = 0;

                        if (yasumu_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result3.Confidence * 100);
                            Trace.WriteLine(dt + " 休む：" + result + "%");
                        }
                    }

                    if (index_next == 3)
                    {
                        if (result4.Confidence > 0.1)
                        {
                            textBlock1.Text = "忘れる：判定中";
                        }

                        if (result4.Confidence > 0.2)
                        {
                            textBlock1.Text = "忘れる：○";
                        }
                        textBlock2.Text = "忘れる：" + result4.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result4.Confidence >= 0.3)
                        {
                            sw_wasureru(true);
                            textBlock1.Text = "忘れる：◎";
                        }

                        if (wasureru_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result4.Confidence * 100);
                            Trace.WriteLine(dt + " 忘れる：" + result + "%");
                            //変な挙動したのでリセットしておく
                            wasureru_time = 0;
                        }
                    }

                    if (index_next == 4)
                    {
                        if (result5.Confidence > 0.1)
                        {
                            textBlock1.Text = "住む：判定中";
                        }

                        if (result5.Confidence > 0.2)
                        {
                            textBlock1.Text = "住む：○";
                        }
                        textBlock2.Text = "住む：" + result5.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result5.Confidence >= 0.3)
                        {
                            sw_sumu(true);
                            textBlock1.Text = "住む：◎";
                        }
                        else sumu_time = 0;

                        if (sumu_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result5.Confidence * 100);
                            Trace.WriteLine(dt + " 住む：" + result + "%");
                        }
                    }

                    if (index_next == 5)
                    {
                        if (result6.Confidence > 0.05)
                        {
                            textBlock1.Text = "病気：判定中";
                        }

                        if (result6.Confidence > 0.16)
                        {
                            textBlock1.Text = "病気：○";
                        }
                        textBlock2.Text = "病気：" + result6.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result6.Confidence >= 0.25)
                        {
                            sw_yamai(true);
                            textBlock1.Text = "病気：◎";
                        }
                        else yamai_time = 0;

                        if (yamai_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result6.Confidence * 100);
                            Trace.WriteLine(dt + " 病気：" + result + "%");
                        }
                    }

                    if (index_next == 6)
                    {
                        if (result7.Confidence > 0.1)
                        {
                            textBlock1.Text = "勉強：判定中";
                        }

                        if (result7.Confidence > 0.2)
                        {
                            textBlock1.Text = "勉強：○";
                        }
                        textBlock2.Text = "勉強：" + result7.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result7.Confidence >= 0.3)
                        {
                            sw_benkyo(true);
                            textBlock1.Text = "勉強：◎";
                        }
                        else benkyo_time = 0;

                        if (benkyo_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result7.Confidence * 100);
                            Trace.WriteLine(dt + " 勉強：" + result + "%");
                        }
                    }

                    if (index_next == 7)
                    {
                        if (result8.Confidence > 0.1)
                        {
                            textBlock1.Text = "作る：判定中";
                        }

                        if (result8.Confidence > 0.2)
                        {
                            textBlock1.Text = "作る：○";
                        }
                        textBlock2.Text = "作る：" + result8.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result8.Confidence >= 0.3)
                        {
                            sw_tukuru(true);
                            textBlock1.Text = "作る：◎";
                        }
                        else tukuru_time = 0;

                        if (tukuru_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result8.Confidence * 100);
                            Trace.WriteLine(dt + " 作る：" + result + "%");
                        }
                    }

                    if (index_next == 8)
                    {
                        if (result9.Confidence > 0.1)
                        {
                            textBlock1.Text = "こんにちは：判定中";
                        }

                        if (result9.Confidence > 0.2)
                        {
                            textBlock1.Text = "こんにちは：○";
                        }
                        textBlock2.Text = "こんにちは：" + result9.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result9.Confidence >= 0.25)
                        {
                            sw_konnitiha(true);
                            textBlock1.Text = "こんにちは：◎";
                        }
                        else konnitiha_time = 0;

                        if (konnitiha_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result9.Confidence * 100);
                            Trace.WriteLine(dt + " こんにちは：" + result + "%");
                        }
                    }

                    if (index_next == 9)
                    {
                        textBlock2.Text = "楽しい：" + result10.Progress.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result10.Progress >= 0.8)
                        {
                            sw_tanosii(true); //右手が上の状態
                        }
                        else if (result10.Progress <= 0.2)
                        {
                            sw_tanosii(false);
                        }
                        else
                        {
                            textBlock1.Text = "楽しい：判定中";
                        }

                        if (tanosii_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result10.Progress * 100);
                            Trace.WriteLine(dt + " 楽しい：" + result + "%");
                        }
                    }

                    if (index_next == 10)
                    {
                        if (result11.Confidence > 0.1)
                        {
                            textBlock1.Text = "集める：判定中";
                        }

                        if (result11.Confidence > 0.2)
                        {
                            textBlock1.Text = "集める：○";
                        }
                        textBlock2.Text = "集める：" + result11.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result11.Confidence >= 0.3)
                        {
                            sw_atumeru(true);
                            textBlock1.Text = "集める：◎";
                        }
                        else atumeru_time = 0;

                        if (atumeru_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result11.Confidence * 100);
                            Trace.WriteLine(dt + " 集める：" + result + "%");
                        }
                    }

                    if (index_next == 11)
                    {
                        if (result12.Confidence > 0.1)
                        {
                            textBlock1.Text = "お疲れ様：判定中";
                        }

                        if (result12.Confidence > 0.2)
                        {
                            textBlock1.Text = "お疲れ様：○";
                        }
                        textBlock2.Text = "お疲れ様：" + result12.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result12.Confidence >= 0.3)
                        {
                            sw_otukaresama(true);
                            textBlock1.Text = "お疲れ様：◎";
                        }
                        else otukaresama_time = 0;

                        if (otukaresama_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result12.Confidence * 100);
                            Trace.WriteLine(dt + " お疲れ様：" + result + "%");
                        }
                    }

                    if (index_next == 12)
                    {
                        if (result13.Confidence > 0.1)
                        {
                            textBlock1.Text = "寝る：判定中";
                        }

                        if (result13.Confidence > 0.2)
                        {
                            textBlock1.Text = "寝る：○";
                        }
                        textBlock2.Text = "寝る：" + result13.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result13.Confidence >= 0.3)
                        {
                            sw_neru(true);
                            textBlock1.Text = "寝る：◎";
                        }
                        else neru_time = 0;

                        if (neru_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result13.Confidence * 100);
                            Trace.WriteLine(dt + " 寝る：" + result + "%");
                        }
                    }

                    if (index_next == 13)
                    {
                        if (result14.Confidence > 0.1)
                        {
                            textBlock1.Text = "携帯電話：判定中";
                        }

                        if (result14.Confidence > 0.2)
                        {
                            textBlock1.Text = "携帯電話：○";
                        }
                        textBlock2.Text = "携帯電話：" + result14.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result14.Confidence >= 0.3)
                        {
                            sw_keitaidenwa(true);
                            textBlock1.Text = "携帯電話：◎";
                        }
                        else keitaidenwa_time = 0;

                        if (keitaidenwa_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result14.Confidence * 100);
                            Trace.WriteLine(dt + " 携帯電話：" + result + "%");
                        }
                    }

                    if (index_next == 14)
                    {
                        if (result15.Confidence > 0.1)
                        {
                            textBlock1.Text = "マスク：判定中";
                        }

                        if (result15.Confidence > 0.2)
                        {
                            textBlock1.Text = "マスク：○";
                        }
                        textBlock2.Text = "マスク：" + result15.Confidence.ToString();
                        var indexString = index_next + 1;
                        textNumber.Text = indexString + " / " + uriList.Count;
                        if (result15.Confidence >= 0.3)
                        {
                            sw_masuku(true);
                            textBlock1.Text = "マスク：◎";
                        }
                        else masuku_time = 0;

                        if (masuku_time == 20)
                        {
                            DateTime dt = DateTime.Now;
                            double result = Math.Round(result15.Confidence * 100);
                            Trace.WriteLine(dt + " マスク：" + result + "%");
                        }
                    }
                }
            }
        }

        private async void sw_ohayo(bool a)
        {
            ohayo_time++;

            if (ohayo_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;
                
                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }
        private async void sw_omedeto(bool a)
        {
            omedeto_time++;

            if (omedeto_time == 18)
            {
                CaptureScreen();

                image.Visibility = Visibility;
                
                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }
        private async void sw_yasumu(bool a)
        {
            yasumu_time++;

            if (yasumu_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }

        private async void sw_wasureru(bool a)
        {
            wasureru_time++;

            if (wasureru_time == 19)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }

        private async void sw_sumu(bool a)
        {
            sumu_time++;

            if (sumu_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }
        private async void sw_yamai(bool a)
        {
            yamai_time++;

            if (yamai_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }
        private async void sw_benkyo(bool a)
        {
            benkyo_time++;

            if (benkyo_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }
        private async void sw_tukuru(bool a)
        {
            tukuru_time++;

            if (tukuru_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }
        private async void sw_konnitiha(bool a)
        {
            konnitiha_time++;

            if (konnitiha_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }

        private async void sw_tanosii(bool a)
        {
            tanosii_time++;
            if (a && tanosii_time == 10)
            {
                textBlock1.Text = "楽しい：○";
                
                tanosii_flag = true;
                tanosii_time = 0;
            }
            if (!a && tanosii_flag && tanosii_time == 10)
            {
                CaptureScreen();

                textBlock1.Text = "楽しい：◎";
                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;

                tanosii_time = 0;
                tanosii_flag = false;
            }
        }

        private async void sw_atumeru(bool a)
        {
            atumeru_time++;

            if (atumeru_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }

        private async void sw_otukaresama(bool a)
        {
            otukaresama_time++;

            if (otukaresama_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }

        private async void sw_neru(bool a)
        {
            neru_time++;

            if (neru_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                count();

                image.Visibility = Visibility.Hidden;
            }
        }

        private async void sw_keitaidenwa(bool a)
        {
            keitaidenwa_time++;

            if (keitaidenwa_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                //次の問題に遷移
                index_next++;
                _navi.Navigate(uriList[index_next]);

                image.Visibility = Visibility.Hidden;
            }
        }

        private async void sw_masuku(bool a)
        {
            masuku_time++;

            if (masuku_time == 20)
            {
                CaptureScreen();

                image.Visibility = Visibility;

                await Task.Delay(2000);

                image.Visibility = Visibility.Hidden;
            }
        }

        private NavigationService _navi;


        //お手本フレームのみのpage遷移
        private List<Uri> uriList = new List<Uri>() {
            new Uri("/BodyLangModelPage/question1.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question2.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question3.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question4.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question5.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question6.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question7.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question8.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question9.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question10.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question11.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question12.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question13.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question14.xaml",UriKind.Relative),
            new Uri("/BodyLangModelPage/question15.xaml",UriKind.Relative),
        };

        private void count()
        {
            correct_count++;
            MainWindow mw = new MainWindow();
            mw.countC(correct_count);
        }

        //お手本フレーム最初のページ設定
        private void FrameModel_Loaded(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(uriList[0]);
        }

        //先頭ページと最後のページ時のボタン制御
        private void FrameModel_Navigated(object sender, NavigationEventArgs e)
        {
            int index = uriList.IndexOf(_navi.CurrentSource);

            /* if (index <= 0)
                backBtn.IsEnabled = false;
            else
                backBtn.IsEnabled = true;

            if (index + 1 == uriList.Count)
                nextBtn.IsEnabled = false;
            else
                nextBtn.IsEnabled = true; */
        }

        private void TitleBtn_Click(object sender, RoutedEventArgs e)
        {
            var titlePage = new TitlePage();
            NavigationService.Navigate(titlePage);


        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (index_next > 0)
            {
                index_next--;
                _navi.Navigate(uriList[index_next]);

                var indexString = index_next + 1;
                textNumber.Text = indexString + " / " + uriList.Count;
            }
        }


        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            //次へを押した回数を記録する
            next_push_count++;
            MainWindow mw = new MainWindow();
            mw.count(next_push_count);

            CaptureScreen();

            //15問以下の場合の処理
            if (index_next + 1 < uriList.Count)
            {
                index_next++;
                _navi.Navigate(uriList[index_next]);
               
                var indexString = index_next + 1;
                textNumber.Text = indexString + " / " + uriList.Count;

                switch (index_next)
                {
                    case 0:
                        var page1 = new question1();
                        label.Content = page1.TextBox1Str;
                        break;
                    case 1:
                        var page2 = new question2();
                        label.Content = page2.TextBox1Str;
                        break;
                    case 2:
                        var page3 = new question3();
                        label.Content = page3.TextBox1Str;
                        break;
                    case 3:
                        var page4 = new question4();
                        label.Content = page4.TextBox1Str;
                        break;
                    case 4:
                        var page5 = new question5();
                        label.Content = page5.TextBox1Str;
                        break;
                    case 5:
                        var page6 = new question6();
                        label.Content = page6.TextBox1Str;
                        break;
                    case 6:
                        var page7 = new question7();
                        label.Content = page7.TextBox1Str;
                        break;
                    case 7:
                        var page8 = new question8();
                        label.Content = page8.TextBox1Str;
                        break;
                    case 8:
                        var page9 = new question9();
                        label.Content = page9.TextBox1Str;
                        break;
                    case 9:
                        var page10 = new question10();
                        label.Content = page10.TextBox1Str;
                        break;
                    case 10:
                        var page11 = new question11();
                        label.Content = page11.TextBox1Str;
                        break;
                    case 11:
                        var page12 = new question12();
                        label.Content = page12.TextBox1Str;
                        break;
                    case 12:
                        var page13 = new question13();
                        label.Content = page13.TextBox1Str;
                        break;
                    case 13:
                        var page14 = new question14();
                        label.Content = page14.TextBox1Str;
                        break;
                    case 14:
                        var page15 = new question15();
                        label.Content = page15.TextBox1Str;
                        break;
                }
            }
            else
            {
                //１５問目で次へを押したときにいろいろ初期化
                index_next = 0;
                var page1 = new question1();
                label.Content = page1.TextBox1Str;

                textNumber.Text = 0 + " / " + uriList.Count;
                _navi.Navigate(uriList[0]);
            }
        }

        private void CaptureScreen()
        {
            // 画像のサイズを指定し、Bitmapオブジェクトのインスタンスを作成
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            // Bitmap bm = new Bitmap(500, 300);   // 幅500ピクセル × 高さ300ピクセルの場合

            // Graphicsオブジェクトのインスタンスを作成
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm);
            // 画面全体をコピー
            gr.CopyFromScreen(new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 0), bm.Size);

            //時間を取得
            DateTime dt = DateTime.Now;
            //ファイル名を日付にする
            string fileName = "C:\\test1\\"+ dt.Year + "." + dt.Month + "." + 
                              dt.Day + "." + dt.Hour + "時" + dt.Minute + "分" 
                              + dt.Second + "秒.jpg";

            // JPGで保存
            bm.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);

            gr.Dispose();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            HelpModal helpDialog = new HelpModal();

            helpDialog.ShowDialog();
        }
    }
}
