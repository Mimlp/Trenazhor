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

namespace KeyboardTrainer
{
    public partial class Create : Form
    {
        private int _editExerciseId = -1; // -1 означает создание нового, > 0 - редактирование существующего
        private bool _isEditMode = false;

        public Create()
        {
            InitializeComponent();
            InitializeForm();
        }

        // Конструктор для редактирования существующего упражнения
        public Create(int exerciseId)
        {
            InitializeComponent();
            _editExerciseId = exerciseId;
            _isEditMode = true;
            InitializeForm();
            LoadExerciseForEditing(exerciseId);
        }

        private void InitializeForm()
        {
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

            // Изменяем текст кнопки в зависимости от режима
            if (_isEditMode)
            {
                button2.Text = "Сохранить изменения";
                this.Text = "Редактирование упражнения";
            }
            else
            {
                button2.Text = "Добавить упражнение";
                this.Text = "Создание упражнения";
            }
        }

        private void LoadExerciseForEditing(int exerciseId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string query = @"
                        SELECT e.name, e.length, e.text, l.level_name
                        FROM exercise e
                        LEFT JOIN level l ON e.level_id = l.level_id
                        WHERE e.exercise_id = @exerciseId";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@exerciseId", exerciseId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Заполняем поля формы
                                textBox1.Text = reader["name"].ToString();
                                textBox3.Text = reader["length"].ToString();

                                // Устанавливаем текст упражнения
                                string exerciseText = reader["text"].ToString();
                                if (!string.IsNullOrEmpty(exerciseText))
                                {
                                    richTextBox1.Text = exerciseText;
                                    checkBox1.Checked = true;
                                    richTextBox1.Visible = true;
                                }

                                // Устанавливаем уровень сложности
                                string levelName = reader["level_name"].ToString();
                                if (!string.IsNullOrEmpty(levelName))
                                {
                                    int index = comboBox1.FindString(levelName);
                                    if (index != -1)
                                    {
                                        comboBox1.SelectedIndex = index;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Упражнение не найдено", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке упражнения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private readonly string connString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=Trenazhor";

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите уровень сложности", "Ошибка", MessageBoxButtons.OK);
            }
            else
            {
                int len;
                if ((textBox1.Text == String.Empty) || textBox1.Text == "Наименование упражнения")
                {
                    MessageBox.Show("Введите название упражнения");
                }
                else if (!int.TryParse(textBox3.Text, out len))
                {
                    MessageBox.Show("Введите длину упражнения");
                }
                else
                {
                    string ex_name = textBox1.Text;
                    string level_name = comboBox1.SelectedItem.ToString();
                    string ex_text;

                    // Получаем символы для выбранного уровня
                    string symbols = GetSymbolsByLevelName(level_name);

                    if (string.IsNullOrEmpty(symbols))
                    {
                        MessageBox.Show("Для выбранного уровня не найдены символы клавиатурных зон");
                        return;
                    }

                    if (checkBox1.Checked)
                    {
                        if (richTextBox1.Text.Length != len)
                        {
                            MessageBox.Show("Длина введённого текста не соответствует длине упражнения!");
                            return;
                        }

                        ex_text = richTextBox1.Text;

                        // ПРОВЕРКА: все ли символы в тексте соответствуют уровню
                        string invalidChars = CheckTextForLevel(ex_text, symbols);
                        if (!string.IsNullOrEmpty(invalidChars))
                        {
                            MessageBox.Show($"Текст содержит символы, не соответствующие уровню '{level_name}':\n{invalidChars}",
                                          "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        // Генерируем текст из символов зоны клавиатуры
                        ex_text = GenerateTextFromSymbols(symbols, len);
                    }

                    // В зависимости от режима, либо добавляем, либо обновляем упражнение
                    if (_isEditMode)
                    {
                        UpdateExercise(_editExerciseId, ex_name, len, ex_text, level_name);
                    }
                    else
                    {
                        AddExercise(ex_name, len, ex_text, level_name);
                    }
                }
            }
        }

        // Метод для обновления существующего упражнения
        private void UpdateExercise(int exerciseId, string name, int length, string text, string levelName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // Сначала получаем level_id по названию уровня
                    int levelId = GetLevelIdByName(levelName);

                    // Затем обновляем упражнение
                    string updateSql = @"
                        UPDATE exercise 
                        SET name = @name, 
                            length = @length, 
                            text = @text, 
                            level_id = @levelId
                        WHERE exercise_id = @exerciseId";

                    using (var cmd = new NpgsqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@exerciseId", exerciseId);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@length", length);
                        cmd.Parameters.AddWithValue("@text", text);
                        cmd.Parameters.AddWithValue("@levelId", levelId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Упражнение успешно обновлено!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Закрываем форму редактирования
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить упражнение", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении упражнения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для получения ID уровня по названию (вынесен отдельно для переиспользования)
        private int GetLevelIdByName(string levelName)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string sql = "SELECT level_id FROM level WHERE level_name = @levelName";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@levelName", levelName);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        // Остальные методы остаются без изменений...
        private string CheckTextForLevel(string text, string allowedSymbols)
        {
            var allowedSet = new HashSet<char>(allowedSymbols);
            allowedSet.Add(' ');
            allowedSet.Add('\n');
            allowedSet.Add('\r');
            allowedSet.Add('\t');

            var invalidChars = text
                .Where(c => !allowedSet.Contains(c))
                .Distinct()
                .ToArray();

            if (invalidChars.Length == 0)
                return null;

            return $"Недопустимые символы: {string.Join(", ", invalidChars.Select(c => $"'{c}'"))}";
        }

        private string GetSymbolsByLevelName(string levelName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT kz.symbols
                        FROM keyboard_zone kz
                        INNER JOIN keyboard_zone_level kzl ON kz.zone_id = kzl.zone_id
                        INNER JOIN level l ON kzl.level_id = l.level_id
                        WHERE l.level_name = @levelName";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@levelName", levelName);

                        using (var reader = cmd.ExecuteReader())
                        {
                            var allSymbols = new StringBuilder();
                            while (reader.Read())
                            {
                                allSymbols.Append(reader.GetString(0));
                            }

                            var uniqueSymbols = new string(allSymbols.ToString().Distinct().ToArray());
                            return uniqueSymbols;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении символов: {ex.Message}");
                return null;
            }
        }

        private string GenerateTextFromSymbols(string symbols, int length)
        {
            if (string.IsNullOrEmpty(symbols)) return "";

            var random = new Random();
            var result = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(symbols.Length);
                result.Append(symbols[index]);

                if ((i + 1) % random.Next(3, 8) == 0 && i < length - 1)
                {
                    result.Append(' ');
                }
            }

            return result.ToString();
        }

        private void AddExercise(string name, int length, string text, string levelName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // Сначала получаем level_id по названию уровня
                    int levelId = GetLevelIdByName(levelName);

                    // Затем добавляем упражнение
                    string insertSql = @"
                        INSERT INTO exercise (name, length, text, level_id) 
                        VALUES (@name, @length, @text, @levelId)";

                    using (var cmd = new NpgsqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@length", length);
                        cmd.Parameters.AddWithValue("@text", text);
                        cmd.Parameters.AddWithValue("@levelId", levelId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Упражнение успешно добавлено!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Очищаем поля после успешного добавления
                            textBox1.Text = "";
                            textBox3.Text = "";
                            richTextBox1.Text = "";
                            comboBox1.SelectedIndex = 0;
                            checkBox1.Checked = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении упражнения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExercisesAdmin ea = new ExercisesAdmin();
            ea.FormClosed += (s, args) => this.Close();
            ea.Show();
            this.Hide();
        }
    }
}