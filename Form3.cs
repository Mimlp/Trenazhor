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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color darkBrown = Color.FromArgb(60, 30, 10);// тёмно-коричневый
            Color gold = Color.FromArgb(212, 175, 55);// золотой

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                darkBrown,
                gold,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
