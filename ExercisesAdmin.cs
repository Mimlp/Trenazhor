using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace KeyboardTrainer
{
    public partial class ExercisesAdmin : Form
    {
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=Krendel25;Database=Trenazhor";

        public ExercisesAdmin()
        {
            InitializeComponent();
            InitializeGrid();
            LoadExercisesFromDatabase();
        }

        private void LoadExercisesFromDatabase()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            e.exercise_id as Id,
                            e.name as Name,
                            l.level_name as LevelName,
                            e.length as Length
                        FROM exercise e
                        LEFT JOIN level l ON e.level_id = l.level_id
                        ORDER BY e.exercise_id";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            dataGridView1.Rows.Clear();

                            while (reader.Read())
                            {
                                dataGridView1.Rows.Add(
                                    reader["Id"],
                                    reader["Name"],
                                    reader["LevelName"] ?? "Не указан", // Если level не задан
                                    reader["Length"]
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите упражнение для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dataGridView1.SelectedRows[0];
            int exerciseId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string exerciseName = selectedRow.Cells["Name"].Value.ToString();

            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить упражнение \"{exerciseName}\"?\n\nЭто действие нельзя отменить!",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    DeleteExercise(exerciseId);
                    LoadExercisesFromDatabase(); // Обновляем таблицу после удаления
                    MessageBox.Show("Упражнение успешно удалено.", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteExercise(int exerciseId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Начинаем транзакцию для обеспечения атомарности
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Сначала удаляем связанные записи из game_session
                        string deleteSessionsQuery = "DELETE FROM game_session WHERE exercise_id = @exerciseId";
                        using (var cmd = new NpgsqlCommand(deleteSessionsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@exerciseId", exerciseId);
                            cmd.ExecuteNonQuery();
                        }

                        // Затем удаляем само упражнение из таблицы exercise
                        string deleteExerciseQuery = "DELETE FROM exercise WHERE exercise_id = @exerciseId";
                        using (var cmd = new NpgsqlCommand(deleteExerciseQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@exerciseId", exerciseId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Не удалось удалить упражнение: {ex.Message}");
                    }
                }
            }
        }

        // Остальные методы остаются без изменений...
        private void InitializeGrid()
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;

            var colId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "ID",
                Width = 60,
                Name = "Id"
            };
            var colName = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Название",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Name"
            };
            var colLevel = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LevelName",
                HeaderText = "Уровень сложности",
                Width = 150
            };
            var colLength = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Length",
                HeaderText = "Длина",
                Width = 80
            };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 139, 34);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
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
            row.DefaultCellStyle.BackColor = Color.FromArgb(210, 255, 210);
            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Regular);
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
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
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                int exerciseId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

                // Передаем ID упражнения в форму Create для редактирования
                Create crPanel = new Create(exerciseId);
                crPanel.FormClosed += (s, args) =>
                {
                    this.Show(); // Показываем эту форму снова
                    LoadExercisesFromDatabase(); // Обновляем список после редактирования
                };
                crPanel.Show();
                this.Hide();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            MainMenuAdm mM = new MainMenuAdm();
            mM.FormClosed += (s, args) => this.Close();
            mM.Show();
            this.Hide();
        }
    }
}