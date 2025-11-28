using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Npgsql;
using System.Security.Cryptography;

namespace KeyboardTrainer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetPlaceholder(textBox1, "Логин");
            SetPlaceholder(textBox2, "Пароль");
            SetPlaceholder(textBox4, "Логин");
            SetPlaceholder(textBox3, "Пароль");
            button1.Paint += Button1_Paint;
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

            if (btn == button1)
            {
                using (Pen pen = new Pen(Color.White, borderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
            else
            {
                using (Pen pen = new Pen(Color.SandyBrown, borderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
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

        //=== КОНЕЦ ВИЗУАЛИЗАЦИИ ===
        //=== АВТОРИЗАЦИЯ ===
        
        private readonly string connString =
            "Host=localhost;Port=5432;Username=postgres;Password=СВОЙ_ПАРОЛЬ;Database=Trenazhor";
        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();
            bool isAdminLogin = checkBox1.Checked;

            if (isAdminLogin)
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(login))
                {
                    MessageBox.Show("Введите логин и пароль администратора.");
                    return;
                }
                if (!login.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Для входа администратора используйте другой логин");
                    return;
                }
                AuthorizeAdmin(login, password);
            }
            else
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Пожалуйста, заполните оба поля.");
                    return;
                }
                AuthorizeUser(login, password);
            }
        }
        private void AuthorizeAdmin(string login, string password)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT a.user_id, a.password
                        FROM app_user a
                        JOIN role r ON a.role_id = r.role_id
                        WHERE LOWER(a.login) = LOWER(@login) 
                        AND LOWER(r.role_name) = 'admin'
                        LIMIT 1;";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("login", login);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Администратор не найден в базе данных.");
                                return;
                            }

                            string dbPassword = reader.GetString(reader.GetOrdinal("password"));
                            int userId = reader.GetInt32(reader.GetOrdinal("user_id"));

                            if (password != dbPassword)
                            {
                                MessageBox.Show("Неверный пароль администратора.");
                                return;
                            }

                            // Успешный вход
                            MessageBox.Show($"Добро пожаловать, администратор!");
                            Form3 adminPanel = new Form3();
                            adminPanel.FormClosed += (s, args) => this.Close();
                            adminPanel.Show();
                            this.Hide();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }
        private void AuthorizeUser(string login, string password)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT a.user_id, a.login, a.password, r.role_name 
                        FROM app_user a 
                        JOIN role r ON a.role_id = r.role_id 
                        WHERE a.login = @login;";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("login", login);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Пользователь не найден.");
                                return;
                            }

                            string dbPassword = reader.GetString(reader.GetOrdinal("password"));
                            string role = reader.GetString(reader.GetOrdinal("role_name"));
                            int userId = reader.GetInt32(reader.GetOrdinal("user_id"));

                            if (password != dbPassword)
                            {
                                MessageBox.Show("Неверный пароль.");
                                return;
                            }

                            if (role.Equals("user", StringComparison.OrdinalIgnoreCase))
                            {
                                Form4 uselForm = new Form4();
                                uselForm.FormClosed += (s, args) => this.Close();
                                uselForm.Show();
                                this.Hide();
                            }
                            else if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show("Для входа администратора установите флажок «Войти как админ».");
                            }
                            else
                            {
                                MessageBox.Show($"Неизвестная роль: {role}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }

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

                    // 3. Добавляем нового пользователя
                    string insertSql = @"
                        INSERT INTO app_user (login, password, role_id) 
                        VALUES (@login, @password, @role_id);";

                    using (var insertCmd = new NpgsqlCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("login", login);
                        insertCmd.Parameters.AddWithValue("password", password);
                        insertCmd.Parameters.AddWithValue("role_id", userRoleId);

                        int rowsAffected = insertCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
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
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
