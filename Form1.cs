using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
namespace Biometric_Door_Unlock
{   public partial class Form1 : Form
    {
    Capture grabber;
    Image<Bgr, Byte> currentFrame;
    Image<Gray, Byte> gray,resul;
    HaarCascade face;
    MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
    Image<Gray, byte> result, TrainedFace = null;
    List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
    List<string> labels = new List<string>();
    List<string> NamePersons = new List<string>();
    int ContTrain, NumLabels, t;
    string name, names = null;
    MCvAvgComp[][] facesDetected = null;
   
        public Form1()
        {
            InitializeComponent();
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                }

            }
            catch (Exception e)
            {
    
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serialPort1.PortName = "COM"+textBox1.Text;
            serialPort1.Open();
            grabber = new Capture();
            grabber.QueryFrame();
            Application.Idle += new EventHandler(FrameGrabber);
        }
        void FrameGrabber(object sender, EventArgs e)
        {
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            gray = currentFrame.Convert<Gray, Byte>();
             facesDetected = gray.DetectHaarCascade(
                face,
                1.2,// reduce         increase detection ( should be  greater than 1)
               10,// reduce           increase detectinon increase performance
               Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DEFAULT,
               new Size(20, 20));
            if (facesDetected[0].Length > 0)
            {
                currentFrame.Draw(facesDetected[0][0].rect, new Bgr(Color.Red), 2);
                result = currentFrame.Copy(facesDetected[0][0].rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                if (trainingImages.ToArray().Length != 0)
                {
                     MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                      EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                      2000,// reduce   increase accuracy
                      ref termCrit);
                    name = recognizer.Recognize(result);
                    currentFrame.Draw(name, ref font, new Point(facesDetected[0][0].rect.X - 2, facesDetected[0][0].rect.Y - 2), new Bgr(Color.LightGreen));
                     serialPort1.WriteLine(name);
                }
 
            }
             pictureBox1.Image = currentFrame.Bitmap;              
       }

        private void button2_Click(object sender, EventArgs e)
        {
            if (facesDetected[0].Length > 0)  

            try
            {
                trainingImages.Add(result);
                labels.Add(textBox2.Text);
                File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");
                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                }
                pictureBox2.Image = result.Bitmap;
             }
            catch
            {
                MessageBox.Show("Enable the face detection first", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        } 
    }
    
}
