using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlRenderer.Demo.Properties;

namespace HtmlRenderer.Demo
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// the private font used for the demo
        /// </summary>
        private readonly PrivateFontCollection _privateFont = new PrivateFontCollection();


        public Form1()
        {
            InitializeComponent();

            // load custom font font into private fonts collection
            _privateFont.AddFontFile(@"c:\Font_Producteev.ttf");
        }

        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data. </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Aqua, 0, 0, 50, 50);

            var font = new Font(_privateFont.Families[0], 20);
            var bla = TextRenderer.MeasureText("heloo: ", font);
            TextRenderer.DrawText(e.Graphics, "heloo: " + '\uF073', font, new Point(50, 50), Color.YellowGreen);
        }
    }
}
