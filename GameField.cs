using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        private int done;
        private int allExLength;
        public GameField()
        {
            InitializeComponent();
            InitializeTrainer();

            this.DoubleBuffered = true;

            this.KeyPreview = true;
            this.KeyPress += Form_KeyPress;

            cursorTimer.Interval = System.Windows.Forms.SystemInformation.CaretBlinkTime;
            cursorTimer.Tick += CursorTimer_Tick;
            cursorTimer.Start();
        }
        private void InitializeTrainer()
        {
            sourceText = "Привет, это тренировочный текст!";
            currentPosition = 0;
            lblText.BackColor = Color.Transparent;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (currentPosition >= sourceText.Length)
            {
                lblText.Text = "Текст завершён!";
                cursorTimer.Stop();
                return;
            }

            lblText.Invalidate();
            cursorVisible = true;
        }

        private void CursorTimer_Tick(object sender, EventArgs e)
        {
            cursorVisible = !cursorVisible;
            lblText.Invalidate();
        }

        private void Form_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            char inputChar = e.KeyChar;
            if (currentPosition < sourceText.Length)
            {
                char expectedChar = sourceText[currentPosition];

                if (char.ToLower(inputChar) == char.ToLower(expectedChar))
                {
                    currentPosition++;
                    err = false;
                }
                else
                {
                    err = true;
                }

                UpdateDisplay();
            }
        }

        private void lblText_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var font = lblText.Font)
            using (var blackBrush = new SolidBrush(Color.Black))
            using (var errBrush = new SolidBrush(Color.Red))
            using (var cursorPen = new Pen(Color.Blue, 2)) 
            using (var underlinePen = new Pen(Color.Gray, 1))
            {
                float x = 0;
                float textHeight = g.MeasureString("X", font).Height;
                float y = (lblText.Height - textHeight) / 2;

                if (currentPosition >= sourceText.Length) return;

                string remainingText = sourceText.Substring(currentPosition);

                float underlineY = lblText.Height / 2f + 10; 
                g.DrawLine(underlinePen, 0, underlineY, lblText.Width, underlineY);
                float cursorXPosition = 0;

                if (err && remainingText.Length > 0)
                {
                    string firstChar = remainingText[0].ToString();
                    g.DrawString(firstChar, font, errBrush, x, y);

                    if (remainingText.Length > 1)
                    {
                        SizeF firstCharSize = g.MeasureString(firstChar, font);
                        g.DrawString(remainingText.Substring(1), font, blackBrush, x + firstCharSize.Width, y);
                    }
                }
                else
                {
                    g.DrawString(remainingText, font, blackBrush, x, y);
                }

                if (cursorVisible && currentPosition < sourceText.Length)
                {
                    g.DrawLine(cursorPen, cursorXPosition, y, cursorXPosition, y + textHeight);
                }
            }
        }
    }
}
