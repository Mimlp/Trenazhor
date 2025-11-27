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
    public partial class Create : Form
    {
        public Create()
        {
            InitializeComponent();
            SetPlaceholder(textBox1, "Наименование упражнения");
            SetPlaceholder(textBox3, "Длина");
            comboBox1.Items.Add("Уровень сложности");
            comboBox1.Items.Add("Новичок");
            comboBox1.Items.Add("Ученик");
            comboBox1.Items.Add("Мастер клавиш");
            comboBox1.Items.Add("Эксперт скорости");
            comboBox1.Items.Add("Ниндзя");
            comboBox1.SelectedIndex = 0;
            richTextBox1.Visible = false;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color darkBrown = Color.FromArgb(255, 108, 110);
            Color gold = Color.FromArgb(255, 197, 72);

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                darkBrown,
                gold,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
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

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                richTextBox1.Visible = true;
            }
            else
            {
                richTextBox1.Visible = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите уровень сложности","Ошибка",MessageBoxButtons.OK);
            }
        }
    }
}
