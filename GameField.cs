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

namespace KeyboardTrainer
{
    public partial class GameField : Form
    {
        private string sourceText;
        private int currentPosition;
        private bool err = false;
        private bool cursorVisible = true;
        private Dictionary<Keys, Button> keyButtons = new Dictionary<Keys, Button>();

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


        public GameField(string text, int maxErrors, int minPressTime, int maxPressTime)
        {
            InitializeComponent();
            InitializeGame();

            this.sourceText = text;
            this.maxErrorsAllowed = maxErrors;
            this.minPressTime = minPressTime;
            this.maxPressTime = maxPressTime;

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
                    MessageBox.Show($"Слишком быстрое нажатие ({pressTimeMs})!\n" +
                                   "Упражнение завершено.", "Внимание",
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //Добавить выход из упражнения и кнопку заново
                }
                else if (maxPressTime > 0 && pressTimeMs > maxPressTime)
                {
                    cursorTimer.Stop();
                    exerciseTimer.Stop();
                    MessageBox.Show($"Слишком медленное нажатие ({pressTimeMs})!\n" +
                                   "Упражнение завершено.", "Внимание",
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //Добавить выход из упражнения и кнопку заново
                }
            }

            if (char.ToLower(e.KeyChar) == char.ToLower(expected))
            {
                currentPosition++;
                averageTime += (pressTimeMs / 1000);

                // Пересчитываем среднюю скорость
                averageSpeed = (currentPosition + 1) / averageTime;
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
                MessageBox.Show($"Превышено максимальное количество ошибок ({maxErrorsAllowed})!\n" +
                               "Упражнение завершено.", "Внимание",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //Добавить выход из упражнения и кнопку заново
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

                string timeFormatted = $"{(int)elapsedTime.TotalMinutes:00}:{(int)elapsedTime.TotalSeconds % 60:00}.{elapsedTime.Milliseconds / 100}";

                MessageBox.Show($"Упражнение завершено!\n\n" +
                               $"Время: {timeFormatted}\n" +
                               $"Ошибок: {errorCount}/{maxErrorsAllowed}\n" +
                               $"Точность: {accuracy:F1}%\n" +
                               $"Статус: {(errorCount <= maxErrorsAllowed ? "УСПЕХ" : "НЕУДАЧА")}",
                               "Результат",
                               MessageBoxButtons.OK,
                               errorCount <= maxErrorsAllowed ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            }
        }

        private bool CheckPressTime(DateTime pressTime)
        {
            if (currentPosition == 0) return true; // Первая буква - нет предыдущей для сравнения

            TimeSpan timeSinceLastPress = pressTime - lastKeyPressTime;
            int pressTimeMs = (int)timeSinceLastPress.TotalMilliseconds;

            // Проверяем ограничения
            if (minPressTime > 0 && pressTimeMs < minPressTime)
            {
                return false; // Слишком быстро
            }

            if (maxPressTime > 0 && pressTimeMs > maxPressTime)
            {
                return false; // Слишком медленно
            }

            return true;
        }

        private void UpdateLabels()
        {
            label2.Text = $"Ошибок: {errorCount}/{maxErrorsAllowed}";
            label1.Text = $"Прогресс: {currentPosition}/{sourceText.Length}";
            label4.Text = $"Скорость: {averageSpeed:F1} символ/мс";

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
            int tenths = (int)(elapsedTime.Milliseconds / 100);

            label3.Text = $"Время: {minutes:00}:{seconds:00}.{tenths}";
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

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 ex = new Form2();
            ex.FormClosed += (s, args) => this.Close();
            ex.Show();
            this.Hide();
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
            LeftPinky,
            LeftRing,
            LeftMiddle,
            LeftIndex,
            RightIndex, 
            RightMiddle,
            RightRing,
            RightPinky
        }

        private Color FingerColor(Finger finger)
        {
            switch (finger)
            {
                case Finger.LeftPinky: return Color.LightPink;
                case Finger.LeftRing: return Color.LightSalmon;
                case Finger.LeftMiddle: return Color.LightGreen;
                case Finger.LeftIndex: return Color.LightCyan;
                case Finger.RightIndex: return Color.LightBlue;
                case Finger.RightMiddle: return Color.LightGoldenrodYellow;
                case Finger.RightRing: return Color.Plum;
                case Finger.RightPinky: return Color.Thistle;
                default: return Color.LightGray;
            }
        }

        private void CreateKeyboard()
        {
            keyboardPanel.Controls.Clear();
            keyButtons.Clear();

            int keyWidth = 43;
            int keyHeight = 43;
            int margin = 5;

            void AddKey(string text, Keys key, int col, int row, Finger finger, bool isSpecial = false)
            {
                Button btn = new Button
                {
                    Text = text,
                    Width = keyWidth,
                    Height = keyHeight,
                    Left = col * (keyWidth + margin),
                    Top = row * (keyHeight + margin),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isSpecial ? Color.White : FingerColor(finger),
                    ForeColor = Color.Black
                };
                keyboardPanel.Controls.Add(btn);
                keyButtons[key] = btn;
            }

            AddKey("1", Keys.D1, 0, 0, Finger.LeftPinky, true);
            AddKey("2", Keys.D2, 1, 0, Finger.LeftRing, true);
            AddKey("3", Keys.D3, 2, 0, Finger.LeftMiddle, true);
            AddKey("4", Keys.D4, 3, 0, Finger.LeftIndex, true);
            AddKey("5", Keys.D5, 4, 0, Finger.LeftIndex, true);
            AddKey("6", Keys.D6, 5, 0, Finger.RightIndex, true);
            AddKey("7", Keys.D7, 6, 0, Finger.RightIndex, true);
            AddKey("8", Keys.D8, 7, 0, Finger.RightMiddle, true);
            AddKey("9", Keys.D9, 8, 0, Finger.RightRing, true);
            AddKey("0", Keys.D0, 9, 0, Finger.RightPinky, true);
            AddKey("-", Keys.OemMinus, 10, 0, Finger.RightPinky, true);
            AddKey("=", Keys.Oemplus, 11, 0, Finger.RightPinky, true);

            AddKey("Й", Keys.Q, 0, 1, Finger.LeftPinky);
            AddKey("Ц", Keys.W, 1, 1, Finger.LeftRing);
            AddKey("У", Keys.E, 2, 1, Finger.LeftMiddle);
            AddKey("К", Keys.R, 3, 1, Finger.LeftIndex);
            AddKey("Е", Keys.T, 4, 1, Finger.LeftIndex);
            AddKey("Н", Keys.Y, 5, 1, Finger.RightIndex);
            AddKey("Г", Keys.U, 6, 1, Finger.RightIndex);
            AddKey("Ш", Keys.I, 7, 1, Finger.RightMiddle);
            AddKey("Щ", Keys.O, 8, 1, Finger.RightRing);
            AddKey("З", Keys.P, 9, 1, Finger.RightPinky);
            AddKey("Х", Keys.OemOpenBrackets, 10, 1, Finger.RightPinky);
            AddKey("Ъ", Keys.OemCloseBrackets, 11, 1, Finger.RightPinky);

            AddKey("Ф", Keys.A, 0, 2, Finger.LeftPinky);
            AddKey("Ы", Keys.S, 1, 2, Finger.LeftRing);
            AddKey("В", Keys.D, 2, 2, Finger.LeftMiddle);
            AddKey("А", Keys.F, 3, 2, Finger.LeftIndex);
            AddKey("П", Keys.G, 4, 2, Finger.LeftIndex);
            AddKey("Р", Keys.H, 5, 2, Finger.RightIndex);
            AddKey("О", Keys.J, 6, 2, Finger.RightIndex);
            AddKey("Л", Keys.K, 7, 2, Finger.RightMiddle);
            AddKey("Д", Keys.L, 8, 2, Finger.RightRing);
            AddKey("Ж", Keys.OemSemicolon, 9, 2, Finger.RightPinky);
            AddKey("Э", Keys.OemQuotes, 10, 2, Finger.RightPinky);

            AddKey("Я", Keys.Z, 0, 3, Finger.LeftPinky);
            AddKey("Ч", Keys.X, 1, 3, Finger.LeftRing);
            AddKey("С", Keys.C, 2, 3, Finger.LeftMiddle);
            AddKey("М", Keys.V, 3, 3, Finger.LeftIndex);
            AddKey("И", Keys.B, 4, 3, Finger.LeftIndex);
            AddKey("Т", Keys.N, 5, 3, Finger.RightIndex);
            AddKey("Ь", Keys.M, 6, 3, Finger.RightIndex);
            AddKey("Б", Keys.OemOpenBrackets, 7, 3, Finger.RightMiddle);
            AddKey("Ю", Keys.OemPeriod, 8, 3, Finger.RightRing);
            AddKey(".", Keys.OemQuestion, 9, 3, Finger.RightPinky);

            Button space = new Button
            {
                Text = "Пробел",
                Width = keyWidth * 6 + margin * 5,
                Height = keyHeight,
                Left = 3 * (keyWidth + margin),
                Top = 4 * (keyHeight + margin),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            keyboardPanel.Controls.Add(space);
            keyButtons[Keys.Space] = space;
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyButtons.TryGetValue(e.KeyCode, out Button btn))
            {
                btn.BackColor = Color.Black;
                btn.ForeColor = Color.White;
            }
        }
        private Finger GetFingerByKey(Keys key)
        {
            if ("QAZ".Contains(key.ToString())) return Finger.LeftPinky;
            if ("WSX".Contains(key.ToString())) return Finger.LeftRing;
            if ("EDC".Contains(key.ToString())) return Finger.LeftMiddle;
            if ("RFVTGB".Contains(key.ToString())) return Finger.LeftIndex;
            if ("YHNUJM".Contains(key.ToString())) return Finger.RightIndex;
            if ("IK".Contains(key.ToString())) return Finger.RightMiddle;
            if ("OL".Contains(key.ToString())) return Finger.RightRing;
            return Finger.RightPinky;
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (keyButtons.TryGetValue(e.KeyCode, out Button btn))
            {
                btn.ForeColor = Color.Black;
                btn.BackColor = FingerColor(GetFingerByKey(e.KeyCode));
            }
        }
    }
}
