using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> personNames = new List<string>();

        public Form1()
        {
            InitializeComponent();
            btnTakePhoto.Enabled = false;
            face = new HaarCascade("haarcascade_frontalface_default.xml");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            grabber = new Capture();
            grabber.QueryFrame();
            Application.Idle += FrameGrabber;
            btnTakePhoto.Enabled = true;
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

            imageBoxPreview.Image = result;
        } 

        private void btnTakePhoto_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPersonName.Text))
            {
                lblInfo.Text = "You must provide person name";
                return;
            }

            try
            {
                trainingImages.Add(result);
                personNames.Add(txtPersonName.Text);

                File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length + ",");

                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", personNames.ToArray()[i - 1] + ",");
                }
                lblInfo.Text = $"{txtPersonName.Text} face saved";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}