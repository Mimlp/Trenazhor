using Npgsql;
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
        private string connectionString =
            "Host=localhost;Port=5432;Username=postgres;Password=Krendel25;Database=Trenazhor";
        private List<double> userAccuracyData = new List<double>();
        private List<double> userSpeedData = new List<double>();

        public StatisticsUser()
        {
            InitializeComponent();

            // Проверяем, авторизован ли пользователь
            if (!UserSession.IsLoggedIn())
            {
                MessageBox.Show("Пользователь не авторизован. Пожалуйста, войдите в систему.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Проверяем, не заблокирован ли пользователь
            if (UserSession.IsUserBlocked())
            {
                MessageBox.Show("Ваш аккаунт заблокирован. Обратитесь к администратору.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            InitializeChart();
            InitializeGrid();
            LoadUserData();

            // Устанавливаем чекбоксы только если есть данные
            if (userAccuracyData.Count > 0)
            {
                checkBox1.Checked = true;
                checkBox1_CheckedChanged(null, EventArgs.Empty);
            }
            else if (userSpeedData.Count > 0)
            {
                checkBox2.Checked = true;
                checkBox2_CheckedChanged(null, EventArgs.Empty);
            }
            else
            {
                // Если нет данных, показываем сообщение
                ShowNoDataMessage();
            }
        }

        private void ShowNoDataMessage()
        {
            chart1.Series.Clear();

            // Добавляем текстовую аннотацию на график
            TextAnnotation annotation = new TextAnnotation();
            annotation.Text = "Нет данных для отображения";
            annotation.Font = new Font("Arial", 12, FontStyle.Bold);
            annotation.ForeColor = Color.White;
            annotation.Alignment = ContentAlignment.MiddleCenter;
            annotation.X = 50;
            annotation.Y = 50;
            annotation.Width = 100;
            annotation.Height = 30;
            annotation.IsSizeAlwaysRelative = false;

            if (chart1.Annotations.Count == 0)
                chart1.Annotations.Add(annotation);

            chart1.Invalidate();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            MainMenuUsr menForm = new MainMenuUsr();
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
            dataGridView1.Columns.Clear();

            var colData = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "date_completed",
                HeaderText = "Дата",
                Width = 120,
                MinimumWidth = 100,
                Name = "date_completed"
            };
            var colEx = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "exercise_name",
                HeaderText = "Упражнение",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 150,
                Name = "exercise_name"
            };
            var colSpeed = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "typing_speed",
                HeaderText = "Скорость (зн/мин)",
                Width = 130,
                MinimumWidth = 110,
                Name = "typing_speed"
            };
            var colAccuracy = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "accuracy",
                HeaderText = "Точность (%)",
                Width = 120,
                MinimumWidth = 100,
                Name = "accuracy"
            };
            var colError = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "error_count",
                HeaderText = "Ошибки",
                Width = 80,
                MinimumWidth = 60,
                Name = "error_count"
            };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(67, 202, 128);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colData, colEx, colSpeed, colAccuracy, colError });
        }

        private void LoadUserData()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    // Загружаем данные о сессиях пользователя
                    string query = @"
                        SELECT 
                            gs.date_completed,
                            e.name as exercise_name,
                            gs.typing_speed,
                            (100 - gs.error_percent) as accuracy,
                            gs.error_count,
                            gs.time_spent
                        FROM game_session gs
                        INNER JOIN exercise e ON gs.exercise_id = e.exercise_id
                        WHERE gs.user_id = @user_id
                        ORDER BY gs.date_completed ASC";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", UserSession.UserId);

                        connection.Open();
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable;

                        // Заполняем списки для графиков
                        userAccuracyData.Clear();
                        userSpeedData.Clear();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (row["accuracy"] != DBNull.Value &&
                                double.TryParse(row["accuracy"].ToString(), out double accuracy))
                            {
                                userAccuracyData.Add(accuracy);
                            }

                            if (row["typing_speed"] != DBNull.Value &&
                                double.TryParse(row["typing_speed"].ToString(), out double speed))
                            {
                                userSpeedData.Add(speed);
                            }
                        }
                    }
                }

                // Вычисляем статистику
                CalculateUserStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateUserStatistics()
        {
            try
            {
                if (dataGridView1.Rows.Count == 0)
                {
                    InitializeInfo(0, 0, 0, 0);
                    return;
                }

                double totalSpeed = 0;
                double totalAccuracy = 0;
                double totalTime = 0;
                int count = 0;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Скорость
                    if (dataGridView1.Columns.Contains("typing_speed"))
                    {
                        var speedCell = row.Cells["typing_speed"];
                        if (speedCell != null && speedCell.Value != null &&
                            !Convert.IsDBNull(speedCell.Value) &&
                            double.TryParse(speedCell.Value.ToString(), out double speed))
                        {
                            totalSpeed += speed;
                        }
                    }

                    // Точность
                    if (dataGridView1.Columns.Contains("accuracy"))
                    {
                        var accuracyCell = row.Cells["accuracy"];
                        if (accuracyCell != null && accuracyCell.Value != null &&
                            !Convert.IsDBNull(accuracyCell.Value) &&
                            double.TryParse(accuracyCell.Value.ToString(), out double accuracy))
                        {
                            totalAccuracy += accuracy;
                        }
                    }

                    count++;
                }

                // Получаем среднее время выполнения из DataTable
                double avgTime = CalculateAverageTime();

                double avgSpeed = count > 0 ? Math.Round(totalSpeed / count, 1) : 0;
                double avgAccuracy = count > 0 ? Math.Round(totalAccuracy / count, 1) : 0;

                InitializeInfo(avgSpeed, avgAccuracy, avgTime, count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка вычисления статистики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private double CalculateAverageTime()
        {
            try
            {
                if (dataGridView1.DataSource is DataTable dataTable)
                {
                    double totalTime = 0;
                    int count = 0;

                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["time_spent"] != DBNull.Value &&
                            double.TryParse(row["time_spent"].ToString(), out double time))
                        {
                            totalTime += time;
                            count++;
                        }
                    }

                    return count > 0 ? Math.Round(totalTime / count, 1) : 0;
                }
                return 0;
            }
            catch
            {
                return 0;
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

            flow.Controls.Add(CreateInfoLabel($"Средняя скорость: {avgSpeed} зн./мин"));
            flow.Controls.Add(CreateInfoLabel($"Средняя точность: {avgAccuracy}%"));
            flow.Controls.Add(CreateInfoLabel($"Среднее время на упражнение: {avgTime}с"));
            flow.Controls.Add(CreateInfoLabel($"Количество завершённых упражнений: {completedCount}"));
            infoPanel.Controls.Add(flow);
        }

        private System.Windows.Forms.Label CreateInfoLabel(string text)
        {
            return new System.Windows.Forms.Label
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
            try
            {
                chart1.Series.Clear();
                chart1.Annotations.Clear();

                // Проверяем, есть ли данные для построения графика
                if (values == null || values.Length == 0)
                {
                    ShowNoDataMessage();
                    return;
                }

                // Проверяем, существует ли область "Main"
                if (chart1.ChartAreas.Count == 0)
                {
                    InitializeChart();
                }

                var area = chart1.ChartAreas["Main"];
                area.AxisX.ScaleView.ZoomReset();
                area.AxisY.ScaleView.ZoomReset();

                var series = new Series
                {
                    ChartType = SeriesChartType.Line,
                    Color = Color.Black,
                    BorderWidth = 3,
                    ChartArea = "Main",
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 8,
                    MarkerColor = Color.Red
                };

                for (int i = 0; i < values.Length; i++)
                {
                    series.Points.AddXY(i + 1, values[i]);
                }

                chart1.Series.Add(series);

                // Настраиваем оси
                if (values.Length > 0)
                {
                    area.AxisX.Minimum = 1;
                    area.AxisX.Maximum = Math.Max(values.Length, 2); // Минимум 2 для отображения одной точки
                    area.AxisX.Interval = 1;

                    double minValue = values.Min();
                    double maxValue = values.Max();
                    double padding = (maxValue - minValue) * 0.1;

                    // Если все значения одинаковы (например, всего одна запись или все значения равны)
                    if (Math.Abs(maxValue - minValue) < 0.0001)
                    {
                        area.AxisY.Minimum = Math.Max(0, minValue - 10); // Добавляем небольшой отступ
                        area.AxisY.Maximum = maxValue + 10;
                    }
                    else
                    {
                        area.AxisY.Minimum = Math.Max(0, minValue - padding);
                        area.AxisY.Maximum = maxValue + padding;
                    }
                }

                area.RecalculateAxesScale();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения графика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                if (userAccuracyData.Count > 0)
                {
                    DrawGraph(userAccuracyData.ToArray());
                }
                else
                {
                    ShowNoDataMessage();
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                if (userSpeedData.Count > 0)
                {
                    DrawGraph(userSpeedData.ToArray());
                }
                else
                {
                    ShowNoDataMessage();
                }
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            LoadUserData();

            // Обновляем график в зависимости от выбранного чекбокса
            if (checkBox1.Checked && userAccuracyData.Count > 0)
            {
                DrawGraph(userAccuracyData.ToArray());
            }
            else if (checkBox2.Checked && userSpeedData.Count > 0)
            {
                DrawGraph(userSpeedData.ToArray());
            }
            else
            {
                ShowNoDataMessage();
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || e.Value == DBNull.Value || e.Value.ToString() == "")
                return;

            // Форматирование даты
            if (dataGridView1.Columns[e.ColumnIndex].Name == "date_completed" &&
                e.Value is DateTime)
            {
                e.Value = ((DateTime)e.Value).ToString("dd.MM.yyyy HH:mm");
                e.FormattingApplied = true;
            }

            // Форматирование числовых значений
            if (dataGridView1.Columns[e.ColumnIndex].Name == "typing_speed" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "accuracy")
            {
                if (double.TryParse(e.Value.ToString(), out double value))
                {
                    e.Value = Math.Round(value, 1).ToString();
                    e.FormattingApplied = true;
                }
            }
        }
    }
}