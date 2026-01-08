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
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        public Open()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Open_KeyDown;
            this.KeyUp += Open_KeyUp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Authorization f1 = new Authorization();
            f1.FormClosed += (s, args) => this.Close();
            f1.Show();
            this.Hide();
        }
        private void Open_KeyDown(object sender, KeyEventArgs e)
        {
            if (pressedKeys.Contains(e.KeyCode))
                return; 

            pressedKeys.Add(e.KeyCode);
            HighlightButton(e.KeyCode, true);
        }
        private void Open_KeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
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
                    target.Tag = target.Size;
                    target.Size = new Size(target.Width + 4, target.Height + 4);
                    target.BringToFront();
                }
                else
                {
                    target.BackColor = Color.MistyRose;
                    target.Size = (Size)target.Tag;
                }
            }
            else return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }
    }
}
