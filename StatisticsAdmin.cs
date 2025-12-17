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
using System.Windows.Forms.DataVisualization.Charting;

namespace KeyboardTrainer
{
    public partial class StatisticsAdmin : Form
    {
        public StatisticsAdmin()
        {
            InitializeComponent();
            checkBox1.Checked = true;
            InitializeChart();
            chart1.Invalidate();
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

        private void label2_Click(object sender, EventArgs e)
        {
            Form3 menForm = new Form3();
            menForm.FormClosed += (s, args) => this.Close();
            menForm.Show();
            this.Hide();
        }

        private void InitializeGridForUsers()
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Columns.Clear();

            var colId = new DataGridViewTextBoxColumn { HeaderText = "ID", Width = 60 };
            var colLogin = new DataGridViewTextBoxColumn { HeaderText = "Логин", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 150 };
            var colData = new DataGridViewTextBoxColumn { DataPropertyName = "Data", HeaderText = "Дата", Width = 120, MinimumWidth = 100 };
            var colEx = new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Упражнение", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 150 };
            var colSpeed = new DataGridViewTextBoxColumn { DataPropertyName = "Speed", HeaderText = "Скорость (зн/мин)", Width = 130, MinimumWidth = 110 };
            var colAccuracy = new DataGridViewTextBoxColumn { DataPropertyName = "Accuracy", HeaderText = "Точность (%)", Width = 120, MinimumWidth = 100 };
            var colTime = new DataGridViewTextBoxColumn { DataPropertyName = "Time", HeaderText = "Время выполнения", Width = 120, MinimumWidth = 100 };
            var colError = new DataGridViewTextBoxColumn { DataPropertyName = "Error", HeaderText = "Ошибки", Width = 80, MinimumWidth = 60 };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(67, 202, 128); // зеленый для заголовков
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200); // базовый светло-зеленый
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colId, colLogin, colData, colEx, colSpeed, colAccuracy, colTime, colError });
        }

        private void InitializeGridForExercises()
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Columns.Clear();

            var colId = new DataGridViewTextBoxColumn { HeaderText = "ID", Width = 60 };
            var colName = new DataGridViewTextBoxColumn { HeaderText = "Название", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 150 };
            var colLevel = new DataGridViewTextBoxColumn { HeaderText = "Уровень сложности", Width = 130 };
            var colLength = new DataGridViewTextBoxColumn { HeaderText = "Длина (символы)", Width = 120 };
            var colCompleted = new DataGridViewTextBoxColumn { HeaderText = "Количество прохождений", Width = 120 };
            var colSpeed = new DataGridViewTextBoxColumn { HeaderText = "Средняя скорость (зн/мин)", Width = 120 };
            var colAccuracy = new DataGridViewTextBoxColumn { HeaderText = "Средняя точность (%)", Width = 120 };
            var colTime = new DataGridViewTextBoxColumn { HeaderText = "Среднее время на упражнение (с)", Width = 120 };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(67, 202, 128); // зеленый для заголовков
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200); // базовый светло-зеленый
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colId, colName, colLevel, colLength, colCompleted, colSpeed, colAccuracy, colTime });
        }

        double[] accuracyDataUser = { 30, 52, 83, 61, 95, 96, 94 };//Графики можете что угодно от чего строить, просто тогда поменяйте label3.Text

        double[] accuracyDataEx = { 110, 260, 470, 280, 290, 300, 310 };

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                SetPlaceholder(textBox1, "Поиск по логину");
                InitializeGridForUsers();
                InitializeInfo(300, 95, 45, 25);//СЮДА ПАРАМЕТРЫ ДЛЯ ПОЛЬЗОВАТЕЛЯ
                label3.Text = "Изменение точности по мере обучения";
                InitializeChart();
                DrawGraph(accuracyDataUser);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                SetPlaceholder(textBox1, "Поиск по упражнению");
                InitializeGridForExercises();
                InitializeInfo(300, 95, 45, 25);//СЮДА ПАРАМЕТРЫ ДЛЯ УПРАЖНЕНИЯ
                label3.Text = "Изменение точности по всем пользователям для выбранного упражнения";
                InitializeChart();
                DrawGraph(accuracyDataEx);
            }
        }

        private void InitializeInfo(double avgSpeed, double avgAccuracy, double avgTime, int completedCount)
        {
            infoPanel.BackColor = Color.FromArgb(90, Color.White);
            infoPanel.Padding = new Padding(15, 30, 15, 30);
            infoPanel.Controls.Clear();

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            this.Controls.Add(infoPanel);
            flow.Controls.Add(CreateInfoLabel($"Средняя скорость: {avgSpeed} зн./мин"));
            flow.Controls.Add(CreateInfoLabel($"Средняя точность: {avgAccuracy}%"));
            flow.Controls.Add(CreateInfoLabel($"Среднее время на упражнение: {avgTime}с"));
            flow.Controls.Add(CreateInfoLabel($"Количество завершённых упражнений: {completedCount}"));
            infoPanel.Controls.Add(flow);
        }
        
        private Label CreateInfoLabel(string text)
        {
            return new Label
            {
                Text = "• " + text,
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 0, 5)
            };
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
        private void InitializeChart()
        {
            chart1.Legends.Clear();
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();

            var area = new ChartArea("Main");

            area.AxisX.LineColor = Color.White;
            area.AxisY.LineColor = Color.White;

            area.AxisX.LineWidth = 2;
            area.AxisY.LineWidth = 2;

            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisX.IsMarginVisible = false;
            area.AxisY.IsMarginVisible = false;

            area.AxisX.LabelStyle.ForeColor = Color.White;
            area.AxisY.LabelStyle.ForeColor = Color.White;

            chart1.ChartAreas.Add(area);

            chart1.BackColor = Color.FromArgb(90, Color.White);
            chart1.ChartAreas[0].BackColor = Color.FromArgb(90, Color.White);
            chart1.ChartAreas[0].BorderColor = Color.FromArgb(90, Color.White);
        }
        private void DrawGraph(double[] values)
        {
            chart1.Series.Clear();
            var area = chart1.ChartAreas["Main"];
            area.AxisX.ScaleView.ZoomReset();
            area.AxisY.ScaleView.ZoomReset();

            var series = new Series
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Black,
                BorderWidth = 3,
                ChartArea = "Main"
            };

            for (int i = 0; i < values.Length; i++)
            {
                series.Points.AddXY(i + 1, values[i]);
            }

            chart1.Series.Add(series);
            area.RecalculateAxesScale();
        }
    }
}
