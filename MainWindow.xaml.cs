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

namespace KinectV2MascotFace
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // Kinect (MultiFrame)
        private KinectSensor kinect;
        private MultiSourceFrameReader multiFrameReader;

        // Color
        private byte[] colorBuffer;
        private WriteableBitmap colorImage;
        private FrameDescription colorFrameDescription;

        // Body
        private Body[] bodies;

        // Image
        Image ImageColor = new Image();

        // MascotFace
        BitmapSource anzuFace = new BitmapImage(new Uri("Assets/anzu-chan.png", UriKind.Relative));
        BitmapSource conohaFace = new BitmapImage(new Uri("Assets/conoha-chan.png", UriKind.Relative));
        BitmapSource claudiaFace = new BitmapImage(new Uri("Assets/claudia-san.png", UriKind.Relative));
        BitmapSource queryFace = new BitmapImage(new Uri("Assets/query-chan.png", UriKind.Relative));
        BitmapSource unityFace = new BitmapImage(new Uri("Assets/unity-chan.png", UriKind.Relative));
        BitmapSource pronamaFace = new BitmapImage(new Uri("Assets/pronama-chan.png", UriKind.Relative));
        BitmapSource selectedFace;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            MouseUp += MainWindow_MouseUp;
        }

        void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // MainWindowをクリックしたとき、表示するマスコットを変更可能にする
            FaceSelctor.Visibility = System.Windows.Visibility.Visible;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Kinectへの接続
            try
            {
                kinect = KinectSensor.GetDefault();
                if (kinect == null)
                {
                    throw new Exception("Kinect v2 が接続されてないみたいorz");
                }

                kinect.Open();

                // ColorImageの初期設定
                colorFrameDescription = kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                colorImage = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96, 96, PixelFormats.Bgra32, null);
                ImageColor.Source = colorImage;

                // Bodyの初期設定
                bodies = new Body[kinect.BodyFrameSource.BodyCount];

                // フレームリーダーを開く (Color / Body)
                multiFrameReader = kinect.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
                multiFrameReader.MultiSourceFrameArrived += multiFrameReader_MultiSourceFrameArrived;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Close();
            }

            // 既定のマスコットを選んでおく
            selectedFace = claudiaFace;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (multiFrameReader != null)
            {
                multiFrameReader.Dispose();
                multiFrameReader = null;
            }
            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
        }

        private void multiFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiFrame = e.FrameReference.AcquireFrame();

            // Colorの取得と表示
            using (var colorFrame = multiFrame.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }

                colorBuffer = new byte[colorFrameDescription.Width * colorFrameDescription.Height * colorFrameDescription.BytesPerPixel];
                colorFrame.CopyConvertedFrameDataToArray(colorBuffer, ColorImageFormat.Bgra);

                ImageColor.Source = BitmapSource.Create(colorFrameDescription.Width, colorFrameDescription.Height, 96, 96,
                    PixelFormats.Bgra32, null, colorBuffer, colorFrameDescription.Width * (int)colorFrameDescription.BytesPerPixel);

                if (!CanvasBody.Children.Contains(ImageColor)) CanvasBody.Children.Add(ImageColor);
            }

            // Bodyの取得と表示
            using (var bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    return;
                }

                bodyFrame.GetAndRefreshBodyData(bodies);
                DrawMascotFace();
            }
        }

        // Body (Joints) のColor画像上への描画
        private void DrawMascotFace()
        {
            // 描画初期化
            CanvasBody.Children.Clear();
            if (!CanvasBody.Children.Contains(ImageColor)) CanvasBody.Children.Add(ImageColor);

            // それぞれのBody毎にマスコットの顔を配置
            foreach (var body in bodies.Where(b => b.IsTracked))
            {
                // 顔         
                Image mascotFace = new Image()
                {
                    Source = selectedFace,
                    Stretch = Stretch.Uniform,
                    Width = 100,
                    Height = 100,
                };

                CameraSpacePoint[] cameraPoints = new CameraSpacePoint[1];
                ColorSpacePoint[] colorPoints = new ColorSpacePoint[1];

                cameraPoints[0] = body.Joints[JointType.Head].Position;

                // 座標変換
                kinect.CoordinateMapper.MapCameraPointsToColorSpace(cameraPoints, colorPoints);
                if (colorPoints[0].X < 0 || colorPoints[0].Y < 0 || cameraPoints[0].Z < 0) return;

                // 顔の大きさを距離によって変える
                var zoom = 4.5 - (double)cameraPoints[0].Z;
                if (zoom < 1) zoom = 1;
                mascotFace.Width = 120 * zoom;
                mascotFace.Height = 120 * zoom;

                // 描画
                mascotFace.SetValue(TopProperty, (double)(colorPoints[0].Y - (mascotFace.Height / 2)));
                mascotFace.SetValue(LeftProperty, (double)(colorPoints[0].X - (mascotFace.Width / 2)));
                CanvasBody.Children.Add(mascotFace);
            }
        }

        // 選択されたマスコットを描画する顔として反映
        private void SelectMascot(object sender, RoutedEventArgs routedEventArgs)
        {
            Mascot selectedMascot = FaceSelctor.GetSelectedFace();
            switch (selectedMascot)
            {
                case Mascot.Anzu:
                    selectedFace = anzuFace;
                    break;
                case Mascot.Conoha:
                    selectedFace = conohaFace;
                    break;
                case Mascot.Claudia:
                    selectedFace = claudiaFace;
                    break;
                case Mascot.Pronama:
                    selectedFace = pronamaFace;
                    break;
                case Mascot.Query:
                    selectedFace = queryFace;
                    break;
                case Mascot.Unity:
                    selectedFace = unityFace;
                    break;
                default:
                    selectedFace = claudiaFace;
                    break;
            }
        }
    }
}
