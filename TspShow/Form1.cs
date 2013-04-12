using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using ant_C;

namespace TspShow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void PaintLineImage(string series, IList list)
        {
            for (int i = 1; i <= list.Count; i++)
            {
                chart1.Series[series].Points.AddXY(i, list[i - 1]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series2"].Points.Clear();
            Common.CAnt_1List.Clear();
            Common.CAnt_2List.Clear();
            Common.ALPHA_1 = Convert.ToDouble(textBox1.Text);
            Common.BETA_2 = Convert.ToDouble(textBox2.Text);
            MTsp mtsp = new MTsp();
            mtsp.InitData();
            mtsp.Search();
            PaintLineImage("Series1", Common.CAnt_1List);
            PaintLineImage("Series2", Common.CAnt_2List);
            label1.Text = mtsp.m_cBestAnt.m_dbPathLength.ToString();
        }
    }
}
