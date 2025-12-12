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
    public partial class StatisticsUser : Form
    {
        public StatisticsUser()
        {
            InitializeComponent();
            InitializeGrid();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Form4 menForm = new Form4();
            menForm.FormClosed += (s, args) => this.Close();
            menForm.Show();
            this.Hide();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color darkBrown = Color.FromArgb(65, 160, 255);// синий
            Color gold = Color.FromArgb(78, 255, 81);// зеленый

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                darkBrown,
                gold,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void InitializeGrid()
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;


            var colData = new DataGridViewTextBoxColumn { DataPropertyName = "Data", HeaderText = "Дата", Width = 120, MinimumWidth = 100 };
            var colEx = new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Упражнение", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 150 };
            var colSpeed = new DataGridViewTextBoxColumn { DataPropertyName = "Speed", HeaderText = "Скорость (зн/мин)", Width = 130, MinimumWidth = 110 };
            var colAccuracy = new DataGridViewTextBoxColumn { DataPropertyName = "Accuracy", HeaderText = "Точность (%)", Width = 120, MinimumWidth = 100 };
            var colError = new DataGridViewTextBoxColumn { DataPropertyName = "Error", HeaderText = "Ошибки", Width = 80, MinimumWidth = 60 };

            colData.DefaultCellStyle.Format = "HH:mm dd.MM.yyyy";

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(67, 202, 128); // зеленый для заголовков
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200); // базовый светло-зеленый
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colData, colEx, colSpeed, colAccuracy, colError });
        }
    }
}
