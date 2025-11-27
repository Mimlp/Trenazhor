using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardTrainer
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = this.ClientRectangle;
            int w = rect.Width;
            int h = rect.Height;
            if (w <= 0 || h <= 0) return;

            using (var white = new SolidBrush(Color.White))
            {
                g.FillRectangle(white, rect);
            }

            float triangleHeight = h * 2f / 3f;
            float triangleWidth = w * 0.48f;

            PointF[] salad =
            {
            new PointF(0f, h), 
            new PointF(triangleWidth, h),
            new PointF(0f, h - triangleHeight)
        };

            float centerX = w / 2f;
            float centerY = h / 2f;

            float scaleX = 2.2f;
            float scaleY = 2.4f;

            PointF[] pink = new PointF[salad.Length];
            for (int i = 0; i < salad.Length; i++)
            {
                float dx = salad[i].X - centerX;
                float dy = salad[i].Y - centerY;

                float mx = centerX - dx * scaleX + (-w * 0.6f);
                float my = centerY + dy * scaleY + (-h * 0.4f);

                pink[i] = new PointF(mx, my);
            }

            Color saladColor = Color.FromArgb(162, 225, 120);
            Color pinkColor = Color.FromArgb(255, 118, 64);

            using (var brushPink = new SolidBrush(pinkColor))
            {
                g.FillPolygon(brushPink, pink);
            }

            using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                PointF[] shadow = new PointF[salad.Length];
                for (int i = 0; i < salad.Length; i++)
                {
                    shadow[i] = new PointF(salad[i].X + 8f, salad[i].Y + 8f);
                }
                g.FillPolygon(shadowBrush, shadow);
            }

            using (var brushSalad = new SolidBrush(saladColor))
            {
                g.FillPolygon(brushSalad, salad);
            }

            using (var pen = new Pen(Color.FromArgb(80, Color.Black), 1f))
            {
                g.DrawPolygon(pen, salad);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 exercises = new Form2();
            exercises.FormClosed += (s, args) => this.Close();
            exercises.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.FormClosed += (s, args) => this.Close();
            form1.Show();
            this.Hide();
        }
    }
}
