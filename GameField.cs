using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KeyboardTrainer
{
    public partial class GameField : Form
    {
        private string sourceText;
        private int currentPosition;
        private bool err = false;
        private bool cursorVisible = true;
        private Dictionary<Keys, System.Windows.Forms.Button> keyButtons = new Dictionary<Keys, System.Windows.Forms.Button>();

        private int errorCount = 0;
        private int maxErrorsAllowed = 10;
        private int minPressTime = 10;
        private int maxPressTime = 1000;

        private double averageSpeed = 0;
        private double averageTime = 0;

        private Timer exerciseTimer;
        private DateTime startTime;
        private TimeSpan elapsedTime;
        private bool timerStarted = false;
        private DateTime lastKeyPressTime;

        private bool isPaused = false;
        private DateTime pauseStart;
        private TimeSpan maxTime;

        private int userId = UserSession.UserId;
        private int exerciseId;
        private readonly string connString = "Host=localhost;Port=5432;Username=postgres;Password=Krendel25;Database=Trenazhor";


        public GameField(int exerciseId, string text, int maxErrors, int minPressTime, int maxPressTime)
        {
            InitializeComponent();
            InitializeGame();

            this.exerciseId = exerciseId;
            this.sourceText = text;
            this.maxErrorsAllowed = maxErrors;
            this.minPressTime = minPressTime;
            this.maxTime = TimeSpan.FromMilliseconds(maxPressTime * sourceText.Length);

            UpdateLabels();

            pnlTextContainer.Paint += Panel_Paint;
            this.KeyPreview = true;
            this.Controls.Add(pnlTextContainer);
            this.KeyPress += GameField_KeyPress;
            cursorTimer = new Timer();
            cursorTimer.Interval = SystemInformation.CaretBlinkTime;
            cursorTimer.Tick += CursorTimer_Tick;
            cursorTimer.Start();

            exerciseTimer = new Timer();
            exerciseTimer.Interval = 100;
            exerciseTimer.Tick += ExerciseTimer_Tick;

            this.Shown += (s, e) => this.Focus();
            pnlTextContainer.Font = new Font("Segoe UI", 22f, FontStyle.Regular);

            InitializeLegend();
            CreateKeyboard();
            legendPanel.Visible = checkBox1.Checked;
            keyboardPanel.Visible = checkBox1.Checked;
            checkBox1.CheckedChanged += (s, e) =>  legendPanel.Visible = checkBox1.Checked; 
            checkBox1.CheckedChanged += (s, e) => keyboardPanel.Visible = checkBox1.Checked;
            CenterKeyboard();
            this.KeyDown += Form_KeyDown;
            this.KeyUp += Form_KeyUp;
            this.KeyPreview = true;
            this.SetStyle(ControlStyles.Selectable, true);
        }

        private void InitializeGame()
        {
            sourceText = "Привет, это тренировочный текст!";
            currentPosition = 0;
            err = false;
            cursorVisible = true;
        }

        private void CursorTimer_Tick(object sender, EventArgs e)
        {
            cursorVisible = !cursorVisible;
            pnlTextContainer.Invalidate();
        }

        private void ExerciseTimer_Tick(object sender, EventArgs e)
        {
            if (timerStarted)
            {
                elapsedTime = DateTime.Now - startTime;
                UpdateTimeLabel();
            }
        }

        private void GameField_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isPaused)
                return;

            if (currentPosition >= sourceText.Length)
                return;

            if (char.IsControl(e.KeyChar))
                return;

            if (!timerStarted && currentPosition == 0)
            {
                timerStarted = true;
                startTime = DateTime.Now;
                exerciseTimer.Start();
                lastKeyPressTime = DateTime.Now; // Запоминаем время первого нажатия
            }

            DateTime currentPressTime = DateTime.Now;
            char expected = sourceText[currentPosition];

            TimeSpan timeSinceLastPress = currentPressTime - lastKeyPressTime;
            double pressTimeMs = (double)timeSinceLastPress.TotalMilliseconds;


            if (currentPosition > 0)
            {
                if (minPressTime > 0 && pressTimeMs < minPressTime)
                {
                    cursorTimer.Stop();
                    exerciseTimer.Stop();
                    DialogResult result = MessageBox.Show(
                        $"Слишком быстрое нажатие ({pressTimeMs:F0} мс)!\n" +
                        "Упражнение завершено.\n\n" +
                        "Хотите начать заново?",
                        "Внимание",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        // Кнопка "Заново"
                        RestartExercise();
                    }
                    else
                    {
                        // Кнопка "Выйти"
                        SaveSessionAndExit();
                    }
                    return;
                }
                if (elapsedTime > maxTime)
                {
                    cursorTimer.Stop();
                    exerciseTimer.Stop();

                    string timeFormatted = $"{(int)maxTime.TotalMinutes:00}:{(int)maxTime.TotalSeconds % 60:00}";
                    string currentTimeFormatted = $"{(int)elapsedTime.TotalMinutes:00}:{(int)elapsedTime.TotalSeconds % 60:00}";

                    DialogResult result = MessageBox.Show(
                        $"Превышено максимальное время выполнения!\n\n" +
                        $"Затрачено: {currentTimeFormatted}\n" +
                        $"Лимит: {timeFormatted}\n" +
                        $"Макс. время на символ: {maxPressTime} мс\n" +
                        $"Символов: {sourceText.Length}\n\n" +
                        "Хотите начать заново?",
                        "Время вышло!",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        RestartExercise();
                    }
                    else
                    {
                        SaveSessionAndExit();
                    }
                    return;
                }
            }

            if (char.ToLower(e.KeyChar) == char.ToLower(expected))
            {
                currentPosition++;
                averageSpeed = (currentPosition / elapsedTime.TotalSeconds) * 60;
                err = false;
                lastKeyPressTime = currentPressTime;
            }
            else
            {
                errorCount++;
                err = true;
            }
            UpdateLabels();

            if (errorCount > maxErrorsAllowed)
            {
                cursorTimer.Stop();
                exerciseTimer.Stop();
                DialogResult result = MessageBox.Show(
                    $"Превышено максимальное количество ошибок ({maxErrorsAllowed})!\n" +
                    "Упражнение завершено.\n\n" +
                    "Хотите начать заново?",
                    "Внимание",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // Кнопка "Заново"
                    RestartExercise();
                }
                else
                {
                    // Кнопка "Выйти"
                    SaveSessionAndExit();
                }
                return;
            }


            pnlTextContainer.Invalidate();

            if (currentPosition >= sourceText.Length)
            {
                cursorTimer.Stop();

                if (timerStarted)
                {
                    exerciseTimer.Stop();
                    elapsedTime = DateTime.Now - startTime;
                }

                double accuracy = 100.0 * (sourceText.Length - errorCount) / sourceText.Length;

                averageSpeed = (currentPosition / elapsedTime.TotalSeconds) * 60;

                string timeFormatted = $"{(int)elapsedTime.TotalMinutes:00}:{(int)elapsedTime.TotalSeconds % 60:00}";

                DialogResult result = MessageBox.Show($"Упражнение завершено!\n\n" +
                               $"Время: {timeFormatted}\n" +
                               $"Ошибок: {errorCount}/{maxErrorsAllowed}\n" +
                               $"Точность: {accuracy:F1}%\n" +
                               $"Скорость : {averageSpeed:F0}сим/мин\n"+
                               "Хотите начать заново ?",
                               "Результат",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    // Кнопка "Заново"
                    RestartExercise();
                }
                else
                {
                    // Кнопка "Выйти"
                    SaveSessionAndExit();
                }

            }
        }

        //Перезапуск упражнения
        private void RestartExercise()
        {
            SaveGameSession();

            currentPosition = 0;
            errorCount = 0;
            averageSpeed = 0;
            averageTime = 0;
            elapsedTime = TimeSpan.Zero;
            lastKeyPressTime = DateTime.MinValue;

            err = false;
            timerStarted = false;

            cursorTimer.Stop();
            exerciseTimer.Stop();

            cursorTimer.Start();

            UpdateLabels();
            UpdateTimeLabel();
            pnlTextContainer.Invalidate();

            this.Focus();
        }

        //Метод для сохранения сессии и выхода
        private void SaveSessionAndExit()
        {
            // Сохраняем текущую сессию
            SaveGameSession();

            // Возвращаемся в меню
            Form2 ex = new Form2();
            ex.FormClosed += (s, args) => this.Close();
            ex.Show();
            this.Hide();
        }

        // сохранение игровой сессии в БД
        private void SaveGameSession()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"
                    INSERT INTO game_session 
                    (date_completed, typing_speed, error_count, error_percent, user_id, exercise_id, time_spent)
                    VALUES (@date_completed, @typing_speed, @error_count, @error_percent, @user_id, @exercise_id, @time_spent)";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("date_completed", DateTime.Now);
                        cmd.Parameters.AddWithValue("typing_speed", averageSpeed);
                        cmd.Parameters.AddWithValue("error_count", errorCount);
                        cmd.Parameters.AddWithValue("error_percent", 100.0 * errorCount / sourceText.Length);
                        cmd.Parameters.AddWithValue("user_id", userId);
                        cmd.Parameters.AddWithValue("exercise_id", exerciseId);
                        cmd.Parameters.AddWithValue("time_spent", (int)elapsedTime.TotalSeconds);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сохранения сессии: " + ex.Message);
            }
        }

        // Пауза
        private void button1_Click_1(object sender, EventArgs e)
        {
            if(isPaused) return;

            // Фиксируем момент начала паузы
            DateTime pauseStartTime = DateTime.Now;

            cursorTimer.Stop();
            exerciseTimer.Stop();
            isPaused = true;

            DialogResult result = MessageBox.Show(
                "Упражнение приостановлено.\n\nНажмите OK для продолжения.",
                "Пауза",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                // Сколько времени длилась пауза
                TimeSpan pauseDuration = DateTime.Now - pauseStartTime;

                // КОРРЕКТИРУЕМ ВСЕ ВРЕМЕННЫЕ МЕТКИ
                if (timerStarted)
                {
                    startTime = startTime.Add(pauseDuration);
                    lastKeyPressTime = lastKeyPressTime.Add(pauseDuration);

                }

                isPaused = false;
                cursorTimer.Start();
                if (timerStarted)
                {
                    exerciseTimer.Start();
                }
                this.Focus();
            }


        }
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void button2_Click(object sender, EventArgs e)
        {

            Form2 ex = new Form2();
            ex.FormClosed += (s, args) => this.Close();
            ex.Show();
            this.Hide();
        }
        private void UpdateLabels()
        {
            label2.Text = $"Ошибок: {errorCount}/{maxErrorsAllowed}";
            label1.Text = $"Прогресс: {currentPosition}/{sourceText.Length}";
            label4.Text = $"Скорость: {averageSpeed:F0} сим/мин";

            // Меняем цвет в зависимости от количества ошибок
            if (errorCount > maxErrorsAllowed)
                label2.ForeColor = Color.Red;
            else if (errorCount > maxErrorsAllowed * 0.7)
                label2.ForeColor = Color.Orange;
            else
                label2.ForeColor = Color.DarkGreen;
        }

        private void UpdateTimeLabel()
        {
            // Форматируем время: минуты:секунды.десятые
            int totalSeconds = (int)elapsedTime.TotalSeconds;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            label3.Text = $"Время: {minutes:00}:{seconds:00}";
            
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Font font = pnlTextContainer.Font;
            float textHeight = g.MeasureString("X", font).Height;

            float centerY = pnlTextContainer.Height / 2f - textHeight / 2f;
            float cursorX = pnlTextContainer.Width / 2f;

            float underlineY = pnlTextContainer.Height / 2f + textHeight / 2f + 6;
            g.DrawLine(Pens.Gray, 0, underlineY, pnlTextContainer.Width, underlineY);

            if (currentPosition >= sourceText.Length)
            {
                string done = "Упражнение завершено!";
                SizeF size = g.MeasureString(done, font);
                g.DrawString(done, font, Brushes.Green,
                    (pnlTextContainer.Width - size.Width) / 2,
                    centerY);
                return;
            }

            string remaining = sourceText.Substring(currentPosition);

            Brush firstBrush = err ? Brushes.Red : Brushes.MediumBlue;
            g.DrawString(remaining[0].ToString(), font, firstBrush, cursorX, centerY);

            if (remaining.Length > 1)
            {
                float firstWidth = g.MeasureString(remaining[0].ToString(), font).Width;
                g.DrawString(
                    remaining.Substring(1),
                    font,
                    Brushes.Black,
                    cursorX + firstWidth,
                    centerY);
            }

            if (cursorVisible)
            {
                g.DrawLine(
                    Pens.Blue,
                    cursorX - 2,
                    centerY,
                    cursorX - 2,
                    centerY + textHeight);
            }
        }

        

        private void InitializeLegend()
        {
            legendPanel.Controls.Clear();

            var fingerColors = new (string Name, Color Color)[]
            {
                ("Мизинец", Color.LightPink),
                ("Безымянный", Color.LightSalmon),
                ("Средний", Color.LightGreen),
                ("Указ. левый", Color.LightCyan),
                ("Указ. правый", Color.LightBlue),
                ("Большой палец", Color.Plum)
            };

            int y = 0;
            int circleSize = 20;
            int spacing = 27; 

            foreach (var fc in fingerColors)
            {
                Panel square = new Panel
                {
                    Width = circleSize,
                    Height = circleSize,
                    Left = 0, 
                    Top = y,
                    BackColor = fc.Color
                };
                
                Label label = new Label
                {
                    Left = circleSize + 5,
                    Top = y,
                    AutoSize = true,
                    Text = fc.Name
                };

                legendPanel.Controls.Add(square);
                legendPanel.Controls.Add(label);

                y += spacing; 
            }
        }

        enum Finger
        {
            Pinky,
            Ring,
            Middle,
            IndexLeft,
            IndexRight,
            Thumb
        }

        private Color FingerColor(Finger finger)
        {
            switch (finger)
            {
                case Finger.Pinky:
                    return Color.LightPink;
                case Finger.Ring:
                    return Color.PeachPuff;
                case Finger.Middle:
                    return Color.LightGreen;
                case Finger.IndexLeft:
                    return Color.LightCyan;
                case Finger.IndexRight:
                    return Color.LightBlue;
                case Finger.Thumb:
                    return Color.Plum;
                default: return Color.White;
            }
        }
        private Color Darken(Color c, int amount = 35)
        {
            return Color.FromArgb(
                c.A,
                Math.Max(0, c.R - amount),
                Math.Max(0, c.G - amount),
                Math.Max(0, c.B - amount)
            );
        }

        private void AddKey(
            string text,
            Keys key,
            int x,
            int y,
            int width,
            int height,
            Finger? finger = null)
        {
            Color baseColor = finger.HasValue
                ? FingerColor(finger.Value)
                : Color.White;

            System.Windows.Forms.Button btn = new System.Windows.Forms.Button
            {
                Text = text,
                Left = x,
                Top = y,
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = baseColor,

                Tag = baseColor 
            };

            keyboardPanel.Controls.Add(btn);
            keyButtons[key] = btn;
        }


        private void CreateKeyboard()
        {
            keyboardPanel.Controls.Clear();
            keyButtons.Clear();

            int k = 42;
            int h = 42;
            int m = 4;

            int y = 0;

            AddKey("~    Ё", Keys.Oemtilde, 0, y, k, h, Finger.Pinky);
            AddKey("!      1", Keys.D1, 1 * (k + m), y, k, h, Finger.Pinky);
            AddKey("\"     2", Keys.D2, 2 * (k + m), y, k, h, Finger.Ring);
            AddKey("№    3", Keys.D3, 3 * (k + m), y, k, h, Finger.Middle);
            AddKey(";      4", Keys.D4, 4 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey("%    5", Keys.D5, 5 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey(":      6", Keys.D6, 6 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("?     7", Keys.D7, 7 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("*      8", Keys.D8, 8 * (k + m), y, k, h, Finger.Middle);
            AddKey("(      9", Keys.D9, 9 * (k + m), y, k, h, Finger.Ring);
            AddKey(")      0", Keys.D0, 10 * (k + m), y, k, h, Finger.Pinky);
            AddKey("_      -", Keys.OemMinus, 11 * (k + m), y, k, h, Finger.Pinky);
            AddKey("+     =", Keys.Add, 12 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Backspace", Keys.Back, 13 * (k + m), y, k * 2, h);

            y += h + m;
            AddKey("Tab", Keys.Tab, 0, y, k, h, Finger.Pinky);
            AddKey("Й", Keys.Q, 1 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Ц", Keys.W, 2 * (k + m), y, k, h, Finger.Ring);
            AddKey("У", Keys.E, 3 * (k + m), y, k, h, Finger.Middle);
            AddKey("К", Keys.R, 4 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey("Е", Keys.T, 5 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey("Н", Keys.Y, 6 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("Г", Keys.U, 7 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("Ш", Keys.I, 8 * (k + m), y, k, h, Finger.Middle);
            AddKey("Щ", Keys.O, 9 * (k + m), y, k, h, Finger.Ring);
            AddKey("З", Keys.P, 10 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Х", Keys.OemOpenBrackets, 11 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Ъ", Keys.OemCloseBrackets, 12 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Enter", Keys.Enter, 13 * (k + m), y, k * 2, h);

            y += h + m;
            AddKey("CapsLock", Keys.CapsLock, 0, y, k, h, Finger.Pinky);
            AddKey("Ф", Keys.A, 1 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Ы", Keys.S, 2 * (k + m), y, k, h, Finger.Ring);
            AddKey("В", Keys.D, 3 * (k + m), y, k, h, Finger.Middle);
            AddKey("А", Keys.F, 4 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey("П", Keys.G, 5 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey("Р", Keys.H, 6 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("О", Keys.J, 7 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("Л", Keys.K, 8 * (k + m), y, k, h, Finger.Middle);
            AddKey("Д", Keys.L, 9 * (k + m), y, k, h, Finger.Ring);
            AddKey("Ж", Keys.OemSemicolon, 10 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Э", Keys.OemQuotes, 11 * (k + m), y, k, h, Finger.Pinky);


            y += h + m;
            AddKey("Shift", Keys.LShiftKey, 0, y, k * 2, h);
            AddKey("Я", Keys.Z, 2 * (k + m), y, k, h, Finger.Pinky);
            AddKey("Ч", Keys.X, 3 * (k + m), y, k, h, Finger.Ring);
            AddKey("С", Keys.C, 4 * (k + m), y, k, h, Finger.Middle);
            AddKey("М", Keys.V, 5 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey("И", Keys.B, 6 * (k + m), y, k, h, Finger.IndexLeft);
            AddKey("Т", Keys.N, 7 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("Ь", Keys.M, 8 * (k + m), y, k, h, Finger.IndexRight);
            AddKey("Б", Keys.Oem102, 9 * (k + m), y, k, h, Finger.Middle);
            AddKey("Ю", Keys.OemPeriod, 10 * (k + m), y, k, h, Finger.Ring);
            AddKey("/", Keys.OemQuestion, 11 * (k + m), y, k, h, Finger.Ring);
            AddKey("Shift", Keys.RShiftKey, 12 * (k + m), y, k * 2, h);

            y += h + m;
            AddKey("Ctrl", Keys.ControlKey, 0, y, k * 2, h);
            AddKey("Alt", Keys.Menu, 2 * (k + m), y, k * 2, h);
            AddKey("Пробел", Keys.Space, 4 * (k + m), y, k * 6, h, Finger.Thumb);
            AddKey("Alt", Keys.Menu, 10 * (k + m), y, k * 2, h);
            AddKey("Ctrl", Keys.ControlKey, 12 * (k + m), y, k * 2, h);
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyButtons.TryGetValue(e.KeyCode, out var btn))
            {
                if (btn.Tag is Color baseColor)
                    btn.BackColor = Darken(baseColor);
            }
        }

        private void Form_KeyUp(object sender, KeyEventArgs e)
        {
            if (keyButtons.TryGetValue(e.KeyCode, out var btn))
            {
                if (btn.Tag is Color baseColor)
                    btn.BackColor = baseColor;
            }
        }

        private void CenterKeyboard()
        {
            int minX = int.MaxValue;
            int maxX = 0;

            foreach (Control c in keyboardPanel.Controls)
            {
                minX = Math.Min(minX, c.Left);
                maxX = Math.Max(maxX, c.Right);
            }

            int keyboardWidth = maxX - minX;
            int offsetX = (keyboardPanel.Width - keyboardWidth) / 2 - minX;

            foreach (Control c in keyboardPanel.Controls)
            {
                c.Left += offsetX;
            }
        }

        
    }
}
