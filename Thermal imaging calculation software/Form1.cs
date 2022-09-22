using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using HomographySharp;
using Xunit;

namespace ThermalImaging
{
    public partial class Form1 : Form
    {
        Dictionary<string, Image<Bgr, byte>> imgList;
        Rectangle rect;
        Rectangle rect1;
        Rectangle cir1 = new Rectangle(10, 15, 10, 10);
        Rectangle cir2 = new Rectangle(430, 15, 10, 10);
        Point startROI, endROI, startROI2, endROI2;
        bool Selecting, Selecting2, mousedown, mousedown2, Circles, Cirmove;
        float x1, y1, x2, y2, x3, y3, x4, y4, temp;
        float a1, b1, a2, b2, a3, b3, a4, b4;
        int finalPointX, finalPointY, finalPointA, finalPointB;
        int cir2x, cir2y;
        double emissivity, ex, ey, maxTemp, minTemp;
        MathNet.Numerics.LinearAlgebra.Matrix<float> mat = null;
        
        double temp1, temp2, result, heatloss;
        double minTempImg1, maxTempImg1, minTempImg2, maxTempImg2;
        public static string Results = "";
        public static string heatLoss = "";

        HomographyMatrix<float> homographyMatrix = null;

        Point2<float> resultcoor;

        public Form1()
        {
            InitializeComponent();
            Selecting = false;
            Selecting2 = false;
            Circles = false;
            rect = Rectangle.Empty;
            imgList = new Dictionary<string, Image<Bgr, byte>>();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close(); // closes the form.
        }

        private void AddImage(Image<Bgr, byte> img, string key)
        {
            if (!treeView1.Nodes.ContainsKey(key))
            {
                TreeNode node = new TreeNode(key);
                node.Name = key;
                treeView1.Nodes.Add(node);
                treeView1.SelectedNode = node;
            }

            if (!imgList.ContainsKey(key))
            {
                imgList.Add(key, img);
            }
            else
            {
                imgList[key] = img;
            }
        }

        
        private void openInsideNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog(); //open standard display box to allow the user to select a file

                dialog.Filter = "Image Files (*.jpg;*.png;*.bmp;)|*.jpg;*.png;*.bmp;|All Files (*.*)|*.*";// checks to see if the file is an image

                if (dialog.ShowDialog() == DialogResult.OK) //If user clicked ok in the dialog box
                {
                    var img = new Image<Bgr, byte>(dialog.FileName);//create a new image variable and fills it with the chosen file

                    AddImage(img, "InsideNormal"); // add the image to the list of images

                    pictureBox1.Image = img.ToBitmap(); // convert the image to bitmap and displays in within the picture box
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);// display an error message
            }
        }

        

        private void openOutsideNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image Files (*.jpg;*.png;*.bmp;)|*.jpg;*.png;*.bmp;|All Files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var img = new Image<Bgr, byte>(dialog.FileName);
                    AddImage(img, "OutsideNormal");
                    pictureBox2.Image = img.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void openInsideThermalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image Files (*.jpg;*.png;*.bmp;)|*.jpg;*.png;*.bmp;|All Files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var img = new Image<Bgr, byte>(dialog.FileName);
                    AddImage(img, "InsideThermal");
                    pictureBox3.Image = img.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void openOutsideThermalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image Files (*.jpg;*.png;*.bmp;)|*.jpg;*.png;*.bmp;|All Files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var img = new Image<Bgr, byte>(dialog.FileName);
                    AddImage(img, "OutsideThermal");
                    pictureBox4.Image = img.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        

        private void selectROI1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Selecting = true;
        }

        private void selectROI2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Selecting2 = true;
        }




        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Selecting)
            {
                mousedown = true; // stays true for as long as the mouse is held down

                startROI = e.Location; // assigns the vale of startROI to the location of the mouse

                x1 = startROI.X;
                y1 = startROI.Y;
                x4 = startROI.X; // x4 == x1
                y3 = startROI.Y; // y3 == y1

                // public variables used later for the homography
                ex = startROI.X;
                ey = startROI.Y;
            }

            if(Cirmove)
            {
                //saves the points for circle 1 for later calculations
                finalPointX = e.X;
                finalPointY = e.Y;

                //saves the points for circle 2 for later calculations
                finalPointA = cir2x;
                finalPointB = cir2y;

                Cirmove = false;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Selecting)
            {
                int width = Math.Max(startROI.X, e.X) - Math.Min(startROI.X, e.X); // gives the width of the rectangle
                int height = Math.Max(startROI.Y, e.Y) - Math.Min(startROI.Y, e.Y);// gives the height of the rectangle

                rect = new Rectangle(Math.Min(startROI.X, e.X), // creates the instance of the rectangle
                    Math.Min(startROI.Y, e.Y),
                    width,
                    height);

                pictureBox1.Refresh(); // refreshes both picture boxes to draw the new rectangle every time
                pictureBox3.Refresh();

            }

            if (Cirmove)
            {

                cir1 = new Rectangle(e.X, e.Y, 10, 10); // sets circle 1 which will be for picture boxes 1 and 3

                ex = e.X; // sets public variables to be used in sett circles 2
                ey = e.Y;

                setcir2();

                pictureBox1.Refresh(); // refreshs each picturebox constantly 
                pictureBox2.Refresh();
                pictureBox3.Refresh();
                pictureBox4.Refresh();
            }
        }

        private void setcir2()
        {
            // translates the x and y through the Homography matrix
            resultcoor = homographyMatrix.Translate(Convert.ToSingle(ex), Convert.ToSingle(ey)); 

            cir2x = Convert.ToInt32(resultcoor.X); // converts doubles to integers
            cir2y = Convert.ToInt32(resultcoor.Y);

            // sets circle 2 which will be for picture boxes 2 and 4 
            cir2 = new Rectangle(cir2x, cir2y, 10, 10); 
            
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Selecting)
            {
                endROI = e.Location; // sets the endROI to the current location of the mouse

                x2 = endROI.X;
                y2 = endROI.Y;
                
                x3 = endROI.X; // x3 == x2
                y4 = endROI.Y; // y4 == y2

                Selecting = false; // stops the process of drawing the ROI
                mousedown = false;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (mousedown)
            {
                using (Pen pen = new Pen(Color.Violet, 5))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }

            if(Circles)
            {
                using (Pen pen = new Pen(Color.LawnGreen, 5))
                {
                    e.Graphics.DrawEllipse(pen, cir1);
                }
            }
        }

        private void pictureBox2_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (Selecting2)
            {
                mousedown2 = true;
                startROI2 = e.Location;
                a1 = startROI2.X;
                b1 = startROI2.Y;
                a4 = startROI2.X;
                b3 = startROI2.Y;
            }
        }
        
        private void pictureBox2_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (Selecting2)
            {
                int width = Math.Max(startROI2.X, e.X) - Math.Min(startROI2.X, e.X);
                int height = Math.Max(startROI2.Y, e.Y) - Math.Min(startROI2.Y, e.Y);
                rect1 = new Rectangle(Math.Min(startROI2.X, e.X),
                    Math.Min(startROI2.Y, e.Y),
                    width,
                    height);
                pictureBox2.Refresh();
                pictureBox4.Refresh();
            }

            if (Cirmove)
            {
                //var newX = mat.Tranlate(e.X);
                cir2 = new Rectangle(e.X, e.Y, 10, 10);

                pictureBox2.Refresh();
                pictureBox4.Refresh();
            }
        }

        private void pictureBox2_MouseUp_1(object sender, MouseEventArgs e)
        {
            if (Selecting2)
            {
                endROI2 = e.Location;
                a2 = endROI2.X;
                b2 = endROI2.Y;
                a3 = endROI2.X;
                b4 = endROI2.Y;


                Selecting2 = false;
                mousedown2 = false;
            }
        }

        private void pictureBox2_Paint_1(object sender, PaintEventArgs e)
        {
            if (mousedown2)
            {
                using (Pen pen = new Pen(Color.Crimson, 5))
                {
                    e.Graphics.DrawRectangle(pen, rect1);
                }
            }

            if (Circles)
            {
                using (Pen pen = new Pen(Color.OrangeRed, 5))
                {
                    e.Graphics.DrawEllipse(pen, cir2);
                }
            }
        }



        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            if (Selecting)
            {
                mousedown = true;
                startROI = e.Location;
            }

            if (Cirmove)
            {
                Cirmove = false;
            }
        }


        private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        {
            if (Selecting)
            {
                int width = Math.Max(startROI.X, e.X) - Math.Min(startROI.X, e.X);
                int height = Math.Max(startROI.Y, e.Y) - Math.Min(startROI.Y, e.Y);
                rect = new Rectangle(Math.Min(startROI.X, e.X),
                    Math.Min(startROI.Y, e.Y),
                    width,
                    height);
                pictureBox1.Refresh();
            }
        }

        

        private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            if (Selecting)
            {
                Selecting = false;
                mousedown = false;
            }
        }

        

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (mousedown)
            {
                using (Pen pen = new Pen(Color.Violet, 5))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }

            if (Circles)
            {
                using (Pen pen = new Pen(Color.DeepSkyBlue, 5))
                {
                    e.Graphics.DrawEllipse(pen, cir1);
                }
            }
        }

        private void pictureBox4_MouseDown(object sender, MouseEventArgs e)
        {
            if (Selecting2)
            {
                mousedown2 = true;
                startROI2 = e.Location;
            }

            if (Cirmove)
            {
                Cirmove = false;
            }
        }

        private void pictureBox4_MouseMove(object sender, MouseEventArgs e)
        {
            if (Selecting2)
            {
                int width = Math.Max(startROI2.X, e.X) - Math.Min(startROI2.X, e.X);
                int height = Math.Max(startROI2.Y, e.Y) - Math.Min(startROI2.Y, e.Y);
                rect1 = new Rectangle(Math.Min(startROI2.X, e.X),
                    Math.Min(startROI2.Y, e.Y),
                    width,
                    height);
                pictureBox2.Refresh();
                
            }
        }
        
        private void pictureBox4_MouseUp(object sender, MouseEventArgs e)
        {
            if (Selecting2)
            {
                Selecting2 = false;
                mousedown2 = false;
            }
        }
        
        private void pictureBox4_Paint(object sender, PaintEventArgs e)
        {
            if (mousedown2)
            {
                using (Pen pen = new Pen(Color.Crimson, 5))
                {
                    e.Graphics.DrawRectangle(pen, rect1);
                }
            }

            if (Circles)
            {
                using (Pen pen = new Pen(Color.HotPink, 5))
                {
                    e.Graphics.DrawEllipse(pen, cir2);
                }
            }
        }
        private void matrixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            findHomography();
        }

        private void findHomography()
        {
            var srcList = new List<Vector2>(4); // list of points from the first ROI
            var dstList = new List<Vector2>(4); // list of points from the second ROI

            srcList.Add(new Vector2(x1, y1));
            srcList.Add(new Vector2(x2, y2));
            srcList.Add(new Vector2(x3, y3));
            srcList.Add(new Vector2(x4, y4));

            dstList.Add(new Vector2(a1, b1));
            dstList.Add(new Vector2(a2, b2));
            dstList.Add(new Vector2(a3, b3));
            dstList.Add(new Vector2(a4, b4));

            homographyMatrix = Homography.Find(srcList, dstList);//makes Homography matrix from the two lists of points

            /*
            { x1, y1, 1.0d, 0.0d, 0.0d, 0.0d, -a1*x1, -a1*y1 }
            { 0.0d, 0.0d, 0.0d, x1, y1, 1.0d, -b1*x1, -b1*y1 }
            { x2, y2, 1.0d, 0.0d, 0.0d, 0.0d, -a2*x2, -a2*y2 }
            { 0.0d, 0.0d, 0.0d, x2, y2, 1.0d, -b2*x2, -b2*y2 }
            { x3, y3, 1.0d, 0.0d, 0.0d, 0.0d, -a3*x3, -a3*y3 }
            { 0.0d, 0.0d, 0.0d, x3, y3, 1.0d, -b3*x3, -b3*y3 }
            { x4, y4, 1.0d, 0.0d, 0.0d, 0.0d, -a4*x4, -a4*y4 }
            { 0.0d, 0.0d, 0.0d, x4, y4, 1.0d, -b4*x4, -b4*y4 }
            */

            // <x1, a1

            resultcoor = homographyMatrix.Translate(x1, y1);// translates point through
                                                            // the Homography matrix

            Assert.True(Math.Abs(resultcoor.X - a1) < 0.001); //true
            Assert.True(Math.Abs(resultcoor.Y - b1) < 0.001);  //true

            // System.Drawing.PointF
            PointF pointf = resultcoor.ToPointF();
            // System.Numerics.Vector2
            Vector2 vector2 = resultcoor.ToVector2();
            // MathNet.Numerics.LinearAlgebra.Matrix<T>
            MathNet.Numerics.LinearAlgebra.Matrix<float> mat = homographyMatrix.ToMathNetMatrix();

            temp = 1;
            

        }
        
        private void drawPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Circles = true;
        }
        private void movePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cirmove = true;
        } 



        
        private void getThermalConductivityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(temp1 > temp2) //checks which temp is higher so we dont have a negitive value
            {
                heatloss = temp1 - temp2;
            }
            else if(temp1 < temp2)
            {
                heatloss = temp2 - temp1;
            }

            result = (emissivity * -1) * -heatloss; // Fourier Law q = -k∆T
            result = Math.Round(result, 2);
            heatloss = Math.Round(heatloss, 2);
            heatLoss = heatloss.ToString();
            heatLoss = heatLoss + " degrees Fahrenheit";

            Results = result.ToString();
            Results = Results + " watts.";

            Form2 f2 = new Form2(); //creates new form
            f2.Show(); // displays new form
        }
        
        
        private void glassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 0.9;
        }

        private void steelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 0.85;
        }

        private void brickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 0.93;
        }

        private void cementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 0.54;
        }

        private void woodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 0.885;
        }

        private void stoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 1.26;
        }

        private void plasticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 0.84;
        }

        private void plasticBlackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emissivity = 0.95;

        }
        
        
        private void maxAndMinForImage1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3(this); //creates the new form
            f3.Show(); // brings the new form onto the screen
        }
        private void maxAndMinForImage2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 f4 = new Form4(this);
            f4.Show();
        }

        public void temperaturesImage1(double min, double max)
        {
           
            minTempImg1 = min;
            maxTempImg1 = max;
        }

        public void temperaturesImage2(double min, double max)
        {
            minTempImg2 = min;
            maxTempImg2 = max;
        }
        private void getTemperatureOfPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //takes the thermal images from the imgList Dictionary
            var img1 = imgList["InsideThermal"].ToBitmap(); 
            var img2 = imgList["OutsideThermal"].ToBitmap();

            // gets the colour of the specific pixel from bith images
            Color pixel1 = img1.GetPixel(finalPointX, finalPointY);
            Color pixel2 = img2.GetPixel(finalPointA, finalPointB);

            // takes the Red RGB value from the colour of the pixel
            String pixel1colour = pixel1.R.ToString();
            String pixel2colour = pixel2.R.ToString();
            double intensity1 = Convert.ToInt32(pixel1colour);
            double intensity2 = Convert.ToInt32(pixel2colour);

            

            //calculates the temperature at the specific pixel
            temp1 = ((intensity1 / 255) * (maxTempImg1 - minTempImg1)) + minTempImg1;
            temp2 = ((intensity2 / 255) * (maxTempImg2 - minTempImg2)) + minTempImg2;

        }
    }
}
 