using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Npgsql;
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
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=Krendel25;Database=Trenazhor";
        private bool isUsersMode = true;
        private List<double> userAccuracyData = new List<double>();
        private List<double> exerciseAccuracyData = new List<double>();

        public StatisticsAdmin()
        {
            InitializeComponent();
            InitializeChart();
            checkBox1.Checked = true;
            LoadDataForUsers();
            SetPlaceholder(textBox1, "Поиск по логину");
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color darkBrown = Color.FromArgb(65, 160, 255);
            Color gold = Color.FromArgb(78, 255, 81);

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
            MainMenuAdm menForm = new MainMenuAdm();
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

            var colId = new DataGridViewTextBoxColumn
            {
                HeaderText = "ID сессии",
                Width = 80,
                DataPropertyName = "session_id",
                Name = "session_id"
            };
            var colLogin = new DataGridViewTextBoxColumn
            {
                HeaderText = "Логин",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 150,
                DataPropertyName = "login",
                Name = "login"
            };
            var colData = new DataGridViewTextBoxColumn
            {
                HeaderText = "Дата завершения",
                Width = 150,
                DataPropertyName = "date_completed",
                Name = "date_completed"
            };
            var colEx = new DataGridViewTextBoxColumn
            {
                HeaderText = "Упражнение",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 150,
                DataPropertyName = "exercise_name",
                Name = "exercise_name"
            };
            var colSpeed = new DataGridViewTextBoxColumn
            {
                HeaderText = "Скорость (зн/мин)",
                Width = 130,
                MinimumWidth = 110,
                DataPropertyName = "typing_speed",
                Name = "typing_speed"
            };
            var colAccuracy = new DataGridViewTextBoxColumn
            {
                HeaderText = "Точность (%)",
                Width = 120,
                MinimumWidth = 100,
                DataPropertyName = "accuracy",
                Name = "accuracy"
            };
            var colTime = new DataGridViewTextBoxColumn
            {
                HeaderText = "Время выполнения",
                Width = 120,
                MinimumWidth = 100,
                DataPropertyName = "time_spent",
                Name = "time_spent"
            };
            var colError = new DataGridViewTextBoxColumn
            {
                HeaderText = "Ошибки",
                Width = 80,
                MinimumWidth = 60,
                DataPropertyName = "error_count",
                Name = "error_count"
            };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(67, 202, 128);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] {
                colId, colLogin, colData, colEx, colSpeed, colAccuracy, colTime, colError
            });
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

            var colId = new DataGridViewTextBoxColumn
            {
                HeaderText = "ID упражнения",
                Width = 100,
                DataPropertyName = "exercise_id",
                Name = "exercise_id"
            };
            var colName = new DataGridViewTextBoxColumn
            {
                HeaderText = "Название",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 150,
                DataPropertyName = "name",
                Name = "name"
            };
            var colLevel = new DataGridViewTextBoxColumn
            {
                HeaderText = "Уровень сложности",
                Width = 130,
                DataPropertyName = "level_name",
                Name = "level_name"
            };
            var colLength = new DataGridViewTextBoxColumn
            {
                HeaderText = "Длина (символы)",
                Width = 120,
                DataPropertyName = "length",
                Name = "length"
            };
            var colCompleted = new DataGridViewTextBoxColumn
            {
                HeaderText = "Количество прохождений",
                Width = 150,
                DataPropertyName = "completed_count",
                Name = "completed_count"
            };
            var colSpeed = new DataGridViewTextBoxColumn
            {
                HeaderText = "Средняя скорость (зн/мин)",
                Width = 150,
                DataPropertyName = "avg_speed",
                Name = "avg_speed"
            };
            var colAccuracy = new DataGridViewTextBoxColumn
            {
                HeaderText = "Средняя точность (%)",
                Width = 150,
                DataPropertyName = "avg_accuracy",
                Name = "avg_accuracy"
            };
            var colTime = new DataGridViewTextBoxColumn
            {
                HeaderText = "Среднее время (с)",
                Width = 120,
                DataPropertyName = "avg_time",
                Name = "avg_time"
            };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(67, 202, 128);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] {
                colId, colName, colLevel, colLength, colCompleted, colSpeed, colAccuracy, colTime
            });
        }

        // Загрузка данных для пользовательского режима
        private void LoadDataForUsers()
        {
            try
            {
                string searchText = textBox1.ForeColor == Color.Gray ? "" : textBox1.Text;

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            gs.session_id,
                            gs.date_completed,
                            gs.time_spent,
                            gs.typing_speed,
                            gs.error_count,
                            gs.error_percent,
                            (100 - gs.error_percent) as accuracy,
                            au.login,
                            e.name as exercise_name
                        FROM game_session gs
                        INNER JOIN app_user au ON gs.user_id = au.user_id
                        INNER JOIN exercise e ON gs.exercise_id = e.exercise_id
                        WHERE (@search = '' OR au.login ILIKE '%' || @search || '%')
                        ORDER BY gs.date_completed DESC";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@search", searchText);

                        connection.Open();
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable;

                        // Собираем данные для графика точности
                        userAccuracyData.Clear();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (row["accuracy"] != DBNull.Value &&
                                double.TryParse(row["accuracy"].ToString(), out double accuracy))
                            {
                                userAccuracyData.Add(accuracy);
                            }
                        }
                    }
                }

                // Вычисляем статистику для панели информации
                CalculateUserStatistics();

                // Обновляем график
                UpdateGraph();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowNoDataMessage("Ошибка загрузки данных");
            }
        }

        // Загрузка данных для режима упражнений
        private void LoadDataForExercises()
        {
            try
            {
                string searchText = textBox1.ForeColor == Color.Gray ? "" : textBox1.Text;

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            e.exercise_id,
                            e.name,
                            e.length,
                            l.level_name,
                            COUNT(gs.session_id) as completed_count,
                            ROUND(AVG(gs.typing_speed::numeric), 1) as avg_speed,
                            ROUND(AVG((100 - gs.error_percent)::numeric), 1) as avg_accuracy,
                            ROUND(AVG(gs.time_spent::numeric), 1) as avg_time
                        FROM exercise e
                        LEFT JOIN level l ON e.level_id = l.level_id
                        LEFT JOIN game_session gs ON e.exercise_id = gs.exercise_id
                        WHERE (@search = '' OR e.name ILIKE '%' || @search || '%')
                        GROUP BY e.exercise_id, e.name, e.length, l.level_name
                        ORDER BY e.exercise_id";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@search", searchText);

                        connection.Open();
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable;

                        // Собираем данные для графика точности
                        exerciseAccuracyData.Clear();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (row["avg_accuracy"] != DBNull.Value &&
                                row["avg_accuracy"].ToString() != "" &&
                                double.TryParse(row["avg_accuracy"].ToString(), out double accuracy))
                            {
                                exerciseAccuracyData.Add(accuracy);
                            }
                        }
                    }
                }

                // Вычисляем статистику для панели информации
                CalculateExerciseStatistics();

                // Обновляем график
                UpdateGraph();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowNoDataMessage("Ошибка загрузки данных");
            }
        }

        // Вычисление статистики для пользовательского режима
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

                // Используем имена колонок, которые мы задали в InitializeGridForUsers
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Проверяем существование колонки
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

                    if (dataGridView1.Columns.Contains("time_spent"))
                    {
                        var timeCell = row.Cells["time_spent"];
                        if (timeCell != null && timeCell.Value != null &&
                            !Convert.IsDBNull(timeCell.Value) &&
                            double.TryParse(timeCell.Value.ToString(), out double time))
                        {
                            totalTime += time;
                        }
                    }

                    count++;
                }

                double avgSpeed = count > 0 ? Math.Round(totalSpeed / count, 1) : 0;
                double avgAccuracy = count > 0 ? Math.Round(totalAccuracy / count, 1) : 0;
                double avgTime = count > 0 ? Math.Round(totalTime / count, 1) : 0;

                InitializeInfo(avgSpeed, avgAccuracy, avgTime, count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка вычисления статистики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Вычисление статистики для режима упражнений
        private void CalculateExerciseStatistics()
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
                int totalCompleted = 0;
                int count = 0;

                // Используем имена колонок, которые мы задали в InitializeGridForExercises
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Проверяем существование колонки и корректность данных
                    if (dataGridView1.Columns.Contains("avg_speed"))
                    {
                        var speedCell = row.Cells["avg_speed"];
                        if (speedCell != null && speedCell.Value != null &&
                            !Convert.IsDBNull(speedCell.Value) &&
                            speedCell.Value.ToString() != "" &&
                            double.TryParse(speedCell.Value.ToString(), out double speed))
                        {
                            totalSpeed += speed;
                        }
                    }

                    if (dataGridView1.Columns.Contains("avg_accuracy"))
                    {
                        var accuracyCell = row.Cells["avg_accuracy"];
                        if (accuracyCell != null && accuracyCell.Value != null &&
                            !Convert.IsDBNull(accuracyCell.Value) &&
                            accuracyCell.Value.ToString() != "" &&
                            double.TryParse(accuracyCell.Value.ToString(), out double accuracy))
                        {
                            totalAccuracy += accuracy;
                        }
                    }

                    if (dataGridView1.Columns.Contains("avg_time"))
                    {
                        var timeCell = row.Cells["avg_time"];
                        if (timeCell != null && timeCell.Value != null &&
                            !Convert.IsDBNull(timeCell.Value) &&
                            timeCell.Value.ToString() != "" &&
                            double.TryParse(timeCell.Value.ToString(), out double time))
                        {
                            totalTime += time;
                        }
                    }

                    if (dataGridView1.Columns.Contains("completed_count"))
                    {
                        var completedCell = row.Cells["completed_count"];
                        if (completedCell != null && completedCell.Value != null &&
                            !Convert.IsDBNull(completedCell.Value) &&
                            int.TryParse(completedCell.Value.ToString(), out int completed))
                        {
                            totalCompleted += completed;
                        }
                    }

                    count++;
                }

                double avgSpeed = count > 0 ? Math.Round(totalSpeed / count, 1) : 0;
                double avgAccuracy = count > 0 ? Math.Round(totalAccuracy / count, 1) : 0;
                double avgTime = count > 0 ? Math.Round(totalTime / count, 1) : 0;

                InitializeInfo(avgSpeed, avgAccuracy, avgTime, totalCompleted);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка вычисления статистики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                isUsersMode = true;
                checkBox2.Checked = false;
                SetPlaceholder(textBox1, "Поиск по логину");
                InitializeGridForUsers();
                LoadDataForUsers();
                label3.Text = "Изменение точности по мере обучения";
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                isUsersMode = false;
                checkBox1.Checked = false;
                SetPlaceholder(textBox1, "Поиск по упражнению");
                InitializeGridForExercises();
                LoadDataForExercises();
                label3.Text = "Изменение точности по всем пользователям для выбранного упражнения";
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
            // Сохраняем текущий текст, если он не placeholder
            string currentText = box.ForeColor == Color.Gray ? "" : box.Text;

            // Удаляем все существующие обработчики
            box.GotFocus -= TextBox_GotFocus;
            box.LostFocus -= TextBox_LostFocus;

            // Добавляем новые обработчики
            box.GotFocus += TextBox_GotFocus;
            box.LostFocus += TextBox_LostFocus;

            // Устанавливаем текст и цвет
            if (string.IsNullOrWhiteSpace(currentText))
            {
                box.Text = placeholder;
                box.ForeColor = Color.Gray;
            }
            else
            {
                box.Text = currentText;
                box.ForeColor = Color.Black;
            }
        }

        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.ForeColor == Color.Gray)
            {
                box.Text = "";
                box.ForeColor = Color.Black;
            }
        }

        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;
            if (string.IsNullOrWhiteSpace(box.Text))
            {
                box.Text = isUsersMode ? "Поиск по логину" : "Поиск по упражнению";
                box.ForeColor = Color.Gray;
            }
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

        private void UpdateGraph()
        {
            if (isUsersMode)
            {
                if (userAccuracyData.Count > 0)
                {
                    DrawGraph(userAccuracyData.ToArray(), "Точность пользователей по сессиям");
                }
                else
                {
                    ShowNoDataMessage("Нет данных о точности пользователей");
                }
            }
            else
            {
                if (exerciseAccuracyData.Count > 0)
                {
                    DrawGraph(exerciseAccuracyData.ToArray(), "Средняя точность по упражнениям");
                }
                else
                {
                    ShowNoDataMessage("Нет данных о точности по упражнениям");
                }
            }
        }

        private void ShowNoDataMessage(string message)
        {
            chart1.Series.Clear();
            chart1.Annotations.Clear();

            // Добавляем текстовую аннотацию на график
            TextAnnotation annotation = new TextAnnotation();
            annotation.Text = message;
            annotation.Font = new Font("Arial", 10, FontStyle.Bold);
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

        private void DrawGraph(double[] values, string title = "")
        {
            try
            {
                chart1.Series.Clear();
                chart1.Annotations.Clear();

                // Проверяем, есть ли данные для построения графика
                if (values == null || values.Length == 0)
                {
                    ShowNoDataMessage("Нет данных для построения графика");
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
                    area.AxisX.Interval = Math.Max(1, values.Length / 10); // Автоматический интервал

                    double minValue = values.Min();
                    double maxValue = values.Max();
                    double padding = (maxValue - minValue) * 0.1;

                    // Если все значения одинаковы (например, всего одна запись или все значения равны)
                    if (Math.Abs(maxValue - minValue) < 0.0001)
                    {
                        area.AxisY.Minimum = Math.Max(0, minValue - 10);
                        area.AxisY.Maximum = maxValue + 10;
                    }
                    else
                    {
                        area.AxisY.Minimum = Math.Max(0, minValue - padding);
                        area.AxisY.Maximum = maxValue + padding;
                    }
                }

                // Устанавливаем заголовок графика
                if (!string.IsNullOrEmpty(title))
                {
                    chart1.Titles.Clear();
                    var titleElement = new Title(title);
                    titleElement.ForeColor = Color.White;
                    titleElement.Font = new Font("Arial", 10, FontStyle.Bold);
                    chart1.Titles.Add(titleElement);
                }

                area.RecalculateAxesScale();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения графика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик поиска
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.ForeColor != Color.Gray)
            {
                if (isUsersMode)
                    LoadDataForUsers();
                else
                    LoadDataForExercises();
            }
        }

        // Форматирование ячеек DataGridView
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

            // Форматирование числовых значений для пользовательского режима
            if (dataGridView1.Columns[e.ColumnIndex].Name == "typing_speed" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "accuracy")
            {
                if (double.TryParse(e.Value.ToString(), out double value))
                {
                    e.Value = Math.Round(value, 1).ToString();
                    e.FormattingApplied = true;
                }
            }

            // Форматирование числовых значений для режима упражнений
            if (dataGridView1.Columns[e.ColumnIndex].Name == "avg_speed" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "avg_accuracy" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "avg_time")
            {
                if (double.TryParse(e.Value.ToString(), out double value))
                {
                    e.Value = Math.Round(value, 1).ToString();
                    e.FormattingApplied = true;
                }
            }
        }

        private void button_search_Click(object sender, EventArgs e)
        {

        }
    }
}