using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace EmguCV
{
    public partial class Form1 : Form
    {
        private Image<Bgr, Byte> currentFrame;
        private Capture grabber;
        private Image<Gray, byte> gray;
        private HaarCascade face;
        private Image<Gray, byte> result;
        private Image<Gray, byte> TrainedFace;

        public Form1()
        {
            InitializeComponent();
            face = new HaarCascade("haarcascade_frontalface_default.xml");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            grabber = new Capture();
            grabber.QueryFrame();
            Application.Idle += FrameGrabber;
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            currentFrame = grabber.QueryFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);
            gray = currentFrame.Convert<Gray, Byte>();

            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                face,
                1.2,
                10,
                HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new Size(20, 20));

            foreach (MCvAvgComp f in facesDetected[0])
            {
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                currentFrame.Draw(f.rect, new Bgr(Color.Yellow), 1);
            }
            imageBoxFrameGrabber.Image = currentFrame;
        } 

        private void btnTakePhoto_Click(object sender, EventArgs e)
        {
            gray = grabber.QueryGrayFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);

            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                face,
                1.2,
                10,
                HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new Size(20, 20));

            foreach (MCvAvgComp f in facesDetected[0])
            {
                TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                break;
            }

            TrainedFace = result.Resize(100, 100, INTER.CV_INTER_CUBIC);
            imageBoxPreview.Image = TrainedFace;
        }
    }
}