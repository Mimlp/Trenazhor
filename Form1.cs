using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace KeyboardTrainer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetPlaceholder(textBox1, "Логин");
            SetPlaceholder(textBox2, "Пароль");
            SetPlaceholder(textBox4, "Логин");
            SetPlaceholder(textBox3, "Пароль");
            button1.Paint += Button1_Paint;
            button2.Paint += Button1_Paint;
        }

        private void Button1_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 30;
            int borderWidth = 6;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(btn.Width - radius - 1, 0, radius, radius, 270, 90);
            path.AddArc(btn.Width - radius - 1, btn.Height - radius - 1, radius, radius, 0, 90);
            path.AddArc(0, btn.Height - radius - 1, radius, radius, 90, 90);
            path.CloseFigure();

            btn.Region = new Region(path);

            if (btn == button1)
            {
                using (Pen pen = new Pen(Color.White, borderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
            else
            {
                using (Pen pen = new Pen(Color.SandyBrown, borderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }
        private void SetPlaceholder(TextBox box, string placeholder)
        {
            box.ForeColor = Color.Gray;
            box.Text = placeholder;

            box.GotFocus += (s, e) =>
            {
                if (box.Text == placeholder)
                {
                    box.Text = "";
                    box.ForeColor = Color.Black;
                }
            };

            box.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    box.Text = placeholder;
                    box.ForeColor = Color.Gray;
                }
            };
        }
    }
}
