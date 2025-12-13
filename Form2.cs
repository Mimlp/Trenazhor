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

            exercisesPanel.FlowDirection = FlowDirection.TopDown;
            exercisesPanel.WrapContents = false;
            exercisesPanel.AutoScroll = true;
            exercisesPanel.Padding = new Padding(6);
            exercisesPanel.BackColor = Color.WhiteSmoke;

            exercisesPanel.SizeChanged += ExercisesPanel_SizeChanged;

            exercisesPanel.Visible = false;
        }

        private readonly string connString = "Host=localhost;Port=5432;Username=postgres;Password=Krendel25;Database=Trenazhor";
        private readonly string[] levelButtons = new[] { "Новичок", "Ученик", "Мастер клавиш", "Эксперт скорости", "Ниндзя" };

        private void InitializeForm()
        {
            contentPanel.Visible = false;
        }

        private void LevelButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string levelName)
            {
                exercisesPanel.Visible = true;
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
                        SELECT e.exercise_id, e.name, e.length, e.text
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
            foreach (var ex in exercises)
            {
                exercisesPanel.Controls.Add(CreateExerciseCard(ex));
            }
            AdjustCardWidths();
        }
        private Control CreateExerciseCard(ExerciseModel ex)
        {
            var panel = new Panel
            {
                Width = exercisesPanel.ClientSize.Width - 25,
                Height = 70,
                Margin = new Padding(6),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblName = new Label
            {
                Text = ex.Name,
                AutoSize = false,
                Location = new Point(8, 8),
                Size = new Size(panel.Width - 140, 28),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Cursor = Cursors.Hand,
                Tag = ex
            };
            

            var lblInfo = new Label
            {
                Text = $"Длительность: {ex.Length} символов",
                Location = new Point(8, 36),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            var btnStart = new Button
            {
                Text = "Начать",
                Size = new Size(90, 32),
                Location = new Point(panel.Width - 100, (panel.Height - 32) / 2),
                Tag = ex
            };
            btnStart.Click += (s, e) =>
            {
                GameField gf = new GameField();
                gf.FormClosed += (s1, args) => this.Close();
                gf.Show();
                this.Hide();
            };

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblInfo);
            panel.Controls.Add(btnStart);

            exercisesPanel.SizeChanged += (s, e) =>
            {
                panel.Width = exercisesPanel.ClientSize.Width - 25;
                lblName.Size = new Size(panel.Width - 140, 28);
                btnStart.Location = new Point(panel.Width - 100, (panel.Height - btnStart.Height) / 2);
            };

            return panel;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Form4 mainMenu = new Form4();
            mainMenu.Show();
        }

        private void ExercisesPanel_SizeChanged(object sender, EventArgs e)
        {
            AdjustCardWidths();
        }

        private void AdjustCardWidths()
        {
            int targetWidth = Math.Max(200, exercisesPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 10);
            foreach (Control c in exercisesPanel.Controls)
            {
                c.Width = targetWidth;
                var lbl = c.Controls.OfType<Label>().FirstOrDefault();
                var btn = c.Controls.OfType<Button>().FirstOrDefault();
                if (lbl != null) lbl.Size = new Size(c.Width - 140, lbl.Size.Height);
                if (btn != null) btn.Location = new Point(c.Width - 100, btn.Location.Y);
            }
        }
    }

}

    
    class ExerciseModel
    {
        public int ExerciseId { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }
    }

    
