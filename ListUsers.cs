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
    public partial class ListUsers : Form
    {
        public ListUsers()
        {
            InitializeComponent();
            SetPlaceholder(textBox1, "Поиск по логину");
            InitializeGrid();
        }


        private void label1_Click(object sender, EventArgs e)
        {
            Form3 mM = new Form3();
            mM.FormClosed += (s, args) => this.Close();
            mM.Show();
            this.Hide();
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

        private void InitializeGrid()
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;


            var colLogin = new DataGridViewTextBoxColumn { DataPropertyName = "Login", HeaderText = "Логин", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colStatus = new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Статус", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(125, 127, 255); // фиолетовый для заголовков
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(202, 203, 255); // базовый светло-сиреневый
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.RowPrePaint += Dgv_RowPrePaint;
            dataGridView1.SelectionChanged += Dgv_SelectionChanged;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colLogin, colStatus });

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            bool has = dataGridView1.SelectedRows.Count > 0;
            button1.Enabled = has;
            button2.Enabled = has;
            button3.Enabled = has;
        }

        private void Dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dataGridView1.Rows[e.RowIndex];
            row.DefaultCellStyle.BackColor = Color.FromArgb(202, 203, 255); // сиреневый
            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Regular);

            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        }
    }
}
