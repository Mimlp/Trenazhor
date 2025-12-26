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
    public partial class Registration : Form
    {
        public Registration()
        {
            InitializeComponent();
            SetPlaceholder(textBox4, "Логин");
            SetPlaceholder(textBox3, "Пароль");
            button2.Paint += Button1_Paint;
        }

        private void Button1_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 30;
            int borderWidth = 6;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(btn.Width - radius - 1, 0, radius, radius, 270, 90);
            path.AddArc(btn.Width - radius - 1, btn.Height - radius - 1, radius, radius, 0, 90);
            path.AddArc(0, btn.Height - radius - 1, radius, radius, 90, 90);
            path.CloseFigure();

            btn.Region = new Region(path);

            using (Pen pen = new Pen(Color.SandyBrown, borderWidth))
            {
                e.Graphics.DrawPath(pen, path);
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

        private readonly string connString =
            "Host=localhost;Port=5432;Username=postgres;Password=root;Database=Trenazhor";

        private void button2_Click(object sender, EventArgs e)
        {
            string login = textBox4.Text.Trim();
            string password = textBox3.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните оба поля.");
                return;
            }
            if (login.Length < 4)
            {
                MessageBox.Show("Логин должен содержать минимум 4 символа.");
                return;
            }
            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа.");
                return;
            }
            if (login.Length > 12)
            {
                MessageBox.Show("Логин не может содержать больше 12 символов.");
                return;
            }
            if (password.Length > 12)
            {
                MessageBox.Show("Пароль не может содержать больше 12 символов.");
                return;
            }
            RegisterUser(login, password);
        }

        private void RegisterUser(string login, string password)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // 1. Проверяем, не занят ли логин
                    string checkSql = @"SELECT COUNT(*) FROM app_user WHERE LOWER(login) = LOWER(@login);";

                    using (var checkCmd = new NpgsqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("login", login);
                        long userCount = (long)checkCmd.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует.");
                            return;
                        }
                    }

                    // 2. Получаем role_id для роли "user"
                    string roleSql = @"SELECT role_id FROM role WHERE LOWER(role_name) = 'user';";
                    int userRoleId;

                    using (var roleCmd = new NpgsqlCommand(roleSql, conn))
                    {
                        var result = roleCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("Ошибка: роль 'user' не найдена в системе.");
                            return;
                        }
                        userRoleId = (int)result;
                    }

                    // 3. Добавляем нового пользователя и получаем данные
                    string insertSql = @"
                        WITH inserted_user AS (
                            INSERT INTO app_user (login, password, role_id) 
                            VALUES (@login, @password, @role_id)
                            RETURNING user_id, blocked, role_id
                        )
                        SELECT iu.user_id, iu.blocked, r.role_name
                        FROM inserted_user iu
                        JOIN role r ON iu.role_id = r.role_id;";

                    using (var insertCmd = new NpgsqlCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("login", login);
                        insertCmd.Parameters.AddWithValue("password", password);
                        insertCmd.Parameters.AddWithValue("role_id", userRoleId);

                        using (var reader = insertCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(reader.GetOrdinal("user_id"));
                                bool blocked = reader.GetBoolean(reader.GetOrdinal("blocked"));
                                string roleName = reader.GetString(reader.GetOrdinal("role_name"));

                                // Сохраняем данные в UserSession
                                UserSession.SetUser(userId, login, roleName, blocked);

                                MessageBox.Show("Пользователь успешно зарегистрирован!");
                                textBox4.Text = "";
                                textBox3.Text = "";
                                Form4 uselForm = new Form4();
                                uselForm.FormClosed += (s, args) => this.Close();
                                uselForm.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Ошибка при регистрации пользователя.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }
    }
}