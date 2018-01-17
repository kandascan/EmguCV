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
        private Image<Gray, byte> TrainedFace;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> personNames = new List<string>();
        int ContTrain;

        public Form1()
        {
            InitializeComponent();
            btnTakePhoto.Enabled = false;
            face = new HaarCascade("haarcascade_frontalface_default.xml");

            try
            {
                string personInfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = personInfo.Split(',');
                ContTrain = Convert.ToInt16(Labels[0]);
                string loadFace;

                for (int tf = 1; tf < ContTrain + 1; tf++)
                {
                    loadFace = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + loadFace));
                    personNames.Add(Labels[tf]);
                }
                lblInfo.Text = $"Total faces saved in hard drive: {ContTrain}";

            }
            catch (Exception e)
            {
                lblInfo.Text = "There is no saved faces";
            }
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
                string personName = String.Empty;
                var font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);

                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                currentFrame.Draw(f.rect, new Bgr(Color.LightGreen), 1);

                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                        trainingImages.ToArray(),
                        personNames.ToArray(),
                        3000,
                        ref termCrit);

                    personName = recognizer.Recognize(result);
                    currentFrame.Draw(personName, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                }
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
                ContTrain += 1;

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