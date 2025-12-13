using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
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
            InitializeGame();

            pnlTextContainer.Paint += Panel_Paint;

            this.Controls.Add(pnlTextContainer);

            this.KeyPress += GameField_KeyPress;

            cursorTimer = new Timer();
            cursorTimer.Interval = SystemInformation.CaretBlinkTime;
            cursorTimer.Tick += CursorTimer_Tick;
            cursorTimer.Start();

            this.Shown += (s, e) => this.Focus();
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

        private void GameField_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (currentPosition >= sourceText.Length)
                return;

            if (char.IsControl(e.KeyChar))
                return;

            char expected = sourceText[currentPosition];

            if (char.ToLower(e.KeyChar) == char.ToLower(expected))
            {
                currentPosition++;
                err = false;
            }
            else
            {
                err = true;
            }

            pnlTextContainer.Invalidate();
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
                string done = "Текст завершён!";
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
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (keyData == Keys.Shift || keyData == Keys.Control || keyData == Keys.Alt)
        //        return base.ProcessCmdKey(ref msg, keyData);

        //    char c = GetCharFromKey(keyData);
        //    if (c == '\0')
        //        return base.ProcessCmdKey(ref msg, keyData);

        //    HandleInput(c);
        //    return true; 
        //}
        //private void HandleInput(char input)
        //{
        //    if (currentPosition >= sourceText.Length)
        //        return;

        //    char expected = sourceText[currentPosition];

        //    if (char.ToLower(input) == char.ToLower(expected))
        //    {
        //        currentPosition++;
        //        err = false;
        //    }
        //    else
        //    {
        //        err = true;
        //    }

        //    pnlTextContainer.Invalidate();
        //}

        //private char GetCharFromKey(Keys key)
        //{
        //    if (key >= Keys.A && key <= Keys.Z)
        //        return (char)('a' + (key - Keys.A));

        //    if (key >= Keys.D0 && key <= Keys.D9)
        //        return (char)('0' + (key - Keys.D0));

        //    if (key >= Keys.Space)
        //        return ' ';

        //    return '\0';
        //}

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    Form2 ex = new Form2();
        //    ex.FormClosed += (s, args) => this.Close();
        //    ex.Show();
        //    this.Hide();
        //}
    }
}
