using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThermalImaging
{
    public partial class Form3 : Form
    {
        Form1 f1;
        double MaxTemp, MinTemp;

        public Form3(Form1 fm1)
        {
            InitializeComponent();
            f1 = fm1;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (f1 != null)
            {
                MinTemp = (double)numericUpDown1.Value;
                MaxTemp = (double)numericUpDown2.Value;
                f1.temperaturesImage1(MinTemp, MaxTemp);
                this.Close();
            }
        }
    }
}
