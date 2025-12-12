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
    public partial class ExercisesAdmin : Form
    {
        public ExercisesAdmin()
        {
            InitializeComponent();
            InitializeGrid();
            FillDemoRows();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Create crPanel = new Create();
            crPanel.FormClosed += (s, args) => this.Close();
            crPanel.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Create crPanel = new Create();
            crPanel.FormClosed += (s, args) => this.Close();
            crPanel.Show();
            this.Hide();
        }

        private void InitializeGrid()
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;


            var colId = new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 60 };
            var colName = new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Название", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colLevel = new DataGridViewTextBoxColumn { DataPropertyName = "LevelName", HeaderText = "Уровень сложности", Width = 150 };
            var colLength = new DataGridViewTextBoxColumn { DataPropertyName = "Length", HeaderText = "Длина", Width = 80 };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 139, 34); // тёмно-зелёный для заголовков
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200); // базовый светло-зелёный
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(100, 180, 100); 
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.RowPrePaint += Dgv_RowPrePaint;
            dataGridView1.SelectionChanged += Dgv_SelectionChanged;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colId, colName, colLevel, colLength });

            
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            bool has = dataGridView1.SelectedRows.Count > 0;
            button2.Enabled = has;
            button3.Enabled = has;
        }

        private void Dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dataGridView1.Rows[e.RowIndex];
            row.DefaultCellStyle.BackColor = Color.FromArgb(210, 255, 210); // светло-зелёный
            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Regular);

            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void FillDemoRows()//ЭТОТ МЕТОД ДЛЯ ПРОСМОТРА ФРОНТЕНДА, ЗАМЕНИТЬ РЕАЛИЗАЦИЕЙ И УДАЛИТЬ!
        {
            dataGridView1.Rows.Add(1, "Шаблон: Разминка", "Легкий", 60); 
            dataGridView1.Rows.Add(2, "Скоростной набор", "Средний", 120);
            dataGridView1.Rows.Add(3, "Длинная сессия", "Сложный", 300);
            dataGridView1.Rows.Add(4, "Фокус на точность", "Легкий", 90);
            dataGridView1.Rows.Add(5, "Комбинированное", "Средний", 150);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Form1 mM = new Form1();
            mM.FormClosed += (s, args) => this.Close();
            mM.Show();
            this.Hide();
        }
    }
}
