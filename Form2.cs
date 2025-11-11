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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            button1.Click += LevelButton_Click;
            button2.Click += LevelButton_Click;
            button3.Click += LevelButton_Click;
            button4.Click += LevelButton_Click;
            button5.Click += LevelButton_Click;
        }

        private readonly string connString =
            "Host=localhost;Port=5432;Username=postgres;Password=СВОЙ_ПАРОЛЬ;Database=Trenazhor";
        private readonly string[] levelButtons = new[] { "Beginner", "Student", "Master", "Expert", "Ninja" };

        private void InitializeForm()
        {
            contentPanel.Visible = false;
        }

        private void LevelButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string levelName)
            {
                exercisesPanel.Visible = true;
                rtbDescription.Visible = true;
                rtbDescription.Clear();
                exercisesPanel.Controls.Clear();

                LoadExercisesForLevel(levelName);
            }
        }
        private void LoadExercisesForLevel(string levelName)
        {
            List<ExerciseModel> exercises = new List<ExerciseModel>();

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT e.name, e.length, e.text
                        FROM exercise e
                        JOIN level l ON e.level_id = l.level_id
                        WHERE LOWER(l.level_name) = LOWER(@levelName)
                        ORDER BY e.exercise_id;";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("levelName", levelName);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                exercises.Add(new ExerciseModel
                                {
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Length = reader.GetInt32(reader.GetOrdinal("length")),
                                    Text = reader.GetString(reader.GetOrdinal("text"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке упражнений: " + ex.Message);
                return;
            }

            if (exercises.Count == 0)
            {
                var lbl = new Label
                {
                    Text = "Для этого уровня упражнений не найдено.",
                    AutoSize = true,
                    ForeColor = Color.DimGray,
                    Padding = new Padding(6)
                };
                exercisesPanel.Controls.Add(lbl);
                return;
            }
            //foreach (var ex in exercises)
            //{
            //    exercisesPanel.Controls.Add(CreateExerciseCard(ex));
            //}
        }
        //private Control CreateExerciseCard(ExerciseModel ex)
        //{
        //    var panel = new Panel
        //    {
        //        Width = exercisesPanel.ClientSize.Width - 25,
        //        Height = 70,
        //        Margin = new Padding(6),
        //        BackColor = Color.White,
        //        BorderStyle = BorderStyle.FixedSingle
        //    };

        //    var lblName = new Label
        //    {
        //        Text = ex.Name,
        //        AutoSize = false,
        //        Location = new Point(8, 8),
        //        Size = new Size(panel.Width - 140, 28),
        //        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        //        ForeColor = Color.FromArgb(40, 40, 40),
        //        Cursor = Cursors.Hand,
        //        Tag = ex
        //    };
        //    lblName.Click += (s, e) =>
        //    {
        //        rtbDescription.Text = ex.Text ?? "(Описание отсутствует)";
        //    };

        //    var lblInfo = new Label
        //    {
        //        Text = $"Длительность: {ex.Length} символов",
        //        Location = new Point(8, 36),
        //        AutoSize = true,
        //        ForeColor = Color.Gray
        //    };

        //    var btnStart = new Button
        //    {
        //        Text = "Начать",
        //        Size = new Size(90, 32),
        //        Location = new Point(panel.Width - 100, (panel.Height - 32) / 2),
        //        Tag = ex
        //    };
        //    btnStart.Click += (s, e) =>
        //    {
        //        MessageBox.Show($"Запуск упражнения: {ex.Name}");
        //    };

        //    panel.Controls.Add(lblName);
        //    panel.Controls.Add(lblInfo);
        //    panel.Controls.Add(btnStart);

        //    exercisesPanel.SizeChanged += (s, e) =>
        //    {
        //        panel.Width = exercisesPanel.ClientSize.Width - 25;
        //        lblName.Size = new Size(panel.Width - 140, 28);
        //        btnStart.Location = new Point(panel.Width - 100, (panel.Height - btnStart.Height) / 2);
        //    };

        //    return panel;
        //}
    }
}
    
    class ExerciseModel
    {
        public int ExerciseId { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }
    }

    
