using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HtmlRenderer.Demo.Properties;

namespace HtmlRenderer.Demo
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
//            htmlLabel1.Text = Resources.String1;
            panel21.Width = ClientRectangle.Width/2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
//            htmlLabel1.Text = Resources.String2;
            panel21.Width = ClientRectangle.Width - 20;

        }

        private void button3_Click(object sender, EventArgs e)
        {
//            htmlLabel1.Text = Resources.String3;
            panel21.Width = ClientRectangle.Width / 4;

        }
    }
}
