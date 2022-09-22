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
    public partial class Form2 : Form
    {
        Form1 f1;
        public Form2()
        {
            InitializeComponent();
            label2.Text = Form1.Results;
            label3.Text = Form1.heatLoss;
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
