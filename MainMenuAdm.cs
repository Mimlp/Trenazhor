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
    public partial class MainMenuAdm : Form
    {
        public MainMenuAdm()
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
            ExercisesAdmin eaPanel = new ExercisesAdmin();
            eaPanel.FormClosed += (s, args) => this.Close();
            eaPanel.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Authorization regForm = new Authorization();
            regForm.FormClosed += (s, args) => this.Close();
            regForm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ListUsers lu = new ListUsers();
            lu.FormClosed += (s, args) => this.Close();
            lu.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StatisticsAdmin sa = new StatisticsAdmin();
            sa.FormClosed += (s, args) => this.Close();
            sa.Show();
            this.Hide();
        }
    }
}
