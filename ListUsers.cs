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
    public partial class ListUsers : Form
    {
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=Trenazhor";

        public ListUsers()
        {
            InitializeComponent();
            SetPlaceholder(textBox1, "Поиск по логину");
            InitializeGrid();
            LoadUsersFromDatabase();
        }

        private void LoadUsersFromDatabase(string searchText = "")
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            u.login as Login,
                            CASE 
                                WHEN u.blocked = true THEN 'Заблокирован'
                                ELSE 'Активен'
                            END as Status,
                            u.user_id as UserId,
                            u.blocked as IsBlocked,
                            u.role_id as RoleId
                        FROM app_user u
                        WHERE u.role_id != 2 -- исключаем админов (role_id = 2)
                          AND (@searchText = '' OR u.login ILIKE @searchText)
                        ORDER BY u.login";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            dataGridView1.Rows.Clear();

                            while (reader.Read())
                            {
                                DataGridViewRow row = new DataGridViewRow();
                                row.CreateCells(dataGridView1);

                                row.Cells[0].Value = reader["Login"];
                                row.Cells[1].Value = reader["Status"];

                                // Безопасное преобразование значений
                                object userIdObj = reader["UserId"];
                                object blockedObj = reader["IsBlocked"];
                                object roleIdObj = reader["RoleId"];

                                var userInfo = new
                                {
                                    UserId = userIdObj != DBNull.Value ? Convert.ToInt32(userIdObj) : 0,
                                    IsBlocked = blockedObj != DBNull.Value ? Convert.ToBoolean(blockedObj) : false,
                                    RoleId = roleIdObj != DBNull.Value ? Convert.ToInt32(roleIdObj) : 0
                                };

                                row.Tag = userInfo;

                                dataGridView1.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text;

            if (searchText != "Поиск по логину" && searchText.Length >= 0)
            {
                LoadUsersFromDatabase(searchText);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            MainMenuAdm mM = new MainMenuAdm();
            mM.FormClosed += (s, args) => this.Close();
            mM.Show();
            this.Hide();
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

        private void InitializeGrid()
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;

            var colLogin = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Login",
                HeaderText = "Логин",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            var colStatus = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Статус",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(125, 127, 255);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(202, 203, 255);
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(157, 158, 251);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.RowPrePaint += Dgv_RowPrePaint;
            dataGridView1.SelectionChanged += Dgv_SelectionChanged;

            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colLogin, colStatus });

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            bool has = dataGridView1.SelectedRows.Count > 0;
            button1.Enabled = has;
            button2.Enabled = has;
            button3.Enabled = has;
        }

        private void Dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dataGridView1.Rows[e.RowIndex];

            if (row.Tag != null)
            {
                dynamic userInfo = row.Tag;

                // Получаем значения из Tag
                int roleId = userInfo.RoleId;
                bool isBlocked = userInfo.IsBlocked;

                if (isBlocked)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                }
                else if (roleId == 1)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(202, 203, 255);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220);
                }
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(202, 203, 255);
            }

            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Regular);
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                dynamic userInfo = selectedRow.Tag;

                if (userInfo != null)
                {
                    int userId = userInfo.UserId;
                    string login = selectedRow.Cells[0].Value.ToString();

                    DialogResult result = MessageBox.Show(
                        $"Заблокировать пользователя {login}?",
                        "Подтверждение блокировки",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            BlockUser(userId, true);
                            LoadUsersFromDatabase(textBox1.Text == "Поиск по логину" ? "" : textBox1.Text);
                            MessageBox.Show("Пользователь заблокирован", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                dynamic userInfo = selectedRow.Tag;

                if (userInfo != null)
                {
                    int userId = userInfo.UserId;
                    string login = selectedRow.Cells[0].Value.ToString();

                    DialogResult result = MessageBox.Show(
                        $"Разблокировать пользователя {login}?",
                        "Подтверждение разблокировки",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            BlockUser(userId, false);
                            LoadUsersFromDatabase(textBox1.Text == "Поиск по логину" ? "" : textBox1.Text);
                            MessageBox.Show("Пользователь разблокирован", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                dynamic userInfo = selectedRow.Tag;

                if (userInfo != null)
                {
                    int userId = userInfo.UserId;
                    string login = selectedRow.Cells[0].Value.ToString();
                    int roleId = userInfo.RoleId;

                    if (roleId == 2)
                    {
                        MessageBox.Show("Нельзя удалить администратора!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DialogResult result = MessageBox.Show(
                        $"Удалить пользователя {login}?\n\nВнимание: Это действие нельзя отменить!",
                        "Подтверждение удаления",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            DeleteUser(userId);
                            LoadUsersFromDatabase(textBox1.Text == "Поиск по логину" ? "" : textBox1.Text);
                            MessageBox.Show("Пользователь удален", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void BlockUser(int userId, bool block)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE app_user SET blocked = @blocked WHERE user_id = @userId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@blocked", block);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteUser(int userId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "SELECT role_id FROM app_user WHERE user_id = @userId";
                int roleId;

                using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@userId", userId);
                    var result = checkCmd.ExecuteScalar();
                    roleId = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }

                if (roleId == 2)
                {
                    throw new Exception("Нельзя удалить администратора!");
                }

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteSessionsQuery = "DELETE FROM game_session WHERE user_id = @userId";
                        using (var cmd = new NpgsqlCommand(deleteSessionsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        string deleteUserQuery = "DELETE FROM app_user WHERE user_id = @userId";
                        using (var cmd = new NpgsqlCommand(deleteUserQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Не удалось удалить пользователя: {ex.Message}");
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadUsersFromDatabase(textBox1.Text == "Поиск по логину" ? "" : textBox1.Text);
        }

        private void ListUsers_Load(object sender, EventArgs e)
        {
            // Метод загрузки формы
        }
    }
}