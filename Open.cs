using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardTrainer
{
    public partial class Open : Form
    {
        public Open()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Open_KeyDown;
            this.KeyUp += Open_KeyUp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 f1 = new Form1();
            f1.FormClosed += (s, args) => this.Close();
            f1.Show();
            this.Hide();
        }
        private void Open_KeyDown(object sender, KeyEventArgs e)
        {
            HighlightButton(e.KeyCode, true);
        }
        private void Open_KeyUp(object sender, KeyEventArgs e)
        {
            HighlightButton(e.KeyCode, false);
        }
        private void HighlightButton(Keys key, bool pressed)
        {
            Button target = null;

            switch (key)
            {
                case Keys.Z:
                    target = button4;
                    break;
                case Keys.E:
                    target = button3;
                    break;
                case Keys.F:
                    target = button5;
                    break;
                case Keys.N:
                    target = button6;
                    break;
                case Keys.U:
                    target = button7;
                    break;
                case Keys.OemPeriod:
                    target = button8;
                    break;
            }

            if (target != null)
            {
                if (pressed)
                {
                    target.BackColor = Color.Cornsilk;
                    target.Size = new Size(target.Width + 4, target.Height + 4);
                }
                else
                {
                    target.BackColor = Color.MistyRose;
                    target.Size = new Size(target.Width - 4, target.Height - 4);
                }
            }
        }
    }
}
