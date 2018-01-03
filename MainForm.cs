using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace EmguCV
{
    public partial class Form1 : Form
    {
        Image<Bgr, Byte> currentFrame;
        Capture grabber;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            grabber = new Capture();
            grabber.QueryFrame();
            Application.Idle += FrameGrabber;
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            currentFrame = grabber.QueryFrame().Resize(640, 480, INTER.CV_INTER_CUBIC);   
            imageBoxFrameGrabber.Image = currentFrame;
        }
    }
}