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
    public partial class StatisticsUser : Form
    {
        public StatisticsUser()
        {
            InitializeComponent();
            InitializeGrid();
            InitializeInfo(300, 95, 45, 25);//ЭТО НУЖНО ВЫЧИСЛЯТЬ! ПОКА ПРОСТО ПРОБНЫЙ
            InitializeChart();//ДЛЯ ГРАФИКА ТОЖЕ НУЖНО ПРОПИСАТЬ ЛОГИКУ (не появления, а заполнения данными, чтобы сам график высчитывал)
            checkBox1.Checked = true;
            chart1.Invalidate();
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
            Color blue = Color.FromArgb(65, 160, 255);
            Color green = Color.FromArgb(78, 255, 81);

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                blue,
                green,
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

        private void InitializeInfo(double avgSpeed, double avgAccuracy, double avgTime, int completedCount)
        {
            infoPanel.BackColor = Color.FromArgb(90, Color.White);
            infoPanel.Padding = new Padding(15,30,15,30);
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
        double[] accuracyData ={ 30, 52, 83, 61, 95, 96, 94};

        double[] speedData ={ 110, 260, 470, 280, 290, 300, 310};

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                DrawGraph(accuracyData);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                DrawGraph(speedData);
            }
        }

    }
}
