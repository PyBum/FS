using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace FlowerShopManagement
{
    public partial class LoginForm : Form
    {
        public User CurrentUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            Text = "Вход в систему";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя и пароль", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = @"
                    SELECT UserID, Username, FullName, Email, Role, IsActive 
                    FROM Users 
                    WHERE Username = @Username AND Password = @Password";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", txtUsername.Text),
                    new SqlParameter("@Password", txtPassword.Text) 
                };

                DataTable result = DatabaseHelper.ExecuteQuery(query, parameters);

                if (result != null && result.Rows.Count > 0)
                {
                    DataRow userRow = result.Rows[0];
                    bool isActive = Convert.ToBoolean(userRow["IsActive"]);

                    if (!isActive)
                    {
                        MessageBox.Show("Ваша учетная запись неактивна. Пожалуйста, обратитесь к администратору.", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    CurrentUser = new User
                    {
                        UserID = Convert.ToInt32(userRow["UserID"]),
                        Username = userRow["Username"].ToString(),
                        FullName = userRow["FullName"].ToString(),
                        Email = userRow["Email"].ToString(),
                        Role = userRow["Role"].ToString(),
                        IsActive = isActive
                    };

                    // Записываем информацию о входе
                    LogLogin(CurrentUser.UserID);

                    // Закрываем форму входа с результатом DialogResult.OK
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка входа: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LogLogin(int userId)
        {
            try
            {
                string query = @"
                    INSERT INTO UserLogs (UserID, LogType, LogDate, IPAddress)
                    VALUES (@UserID, 'Login', GETDATE(), @IPAddress)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@IPAddress", GetClientIPAddress())
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                // Просто логируем ошибку, но не показываем пользователю
                Console.WriteLine("Ошибка записи лога входа: " + ex.Message);
            }
        }

        private string GetClientIPAddress()
        {
           
            return "127.0.0.1";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
           
            CenterToScreen();

           
            txtUsername.Focus();
        }
    }
}