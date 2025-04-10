using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace FlowerShopManagement
{
    public partial class CategoriesForm : Form
    {
        private int currentCategoryId = 0;

        public CategoriesForm()
        {
            InitializeComponent();
            Text = "Управление категориями";
        }

        private void CategoriesForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            ClearFields();
        }

        private void LoadCategories()
        {
            try
            {
                string query = "SELECT CategoryID, CategoryName, Description FROM Categories ORDER BY CategoryName";
                DataTable categories = DatabaseHelper.ExecuteQuery(query);

                if (categories != null)
                {
                    dgvCategories.DataSource = categories;

                    // Настраиваем столбцы
                    if (dgvCategories.Columns.Contains("CategoryID"))
                        dgvCategories.Columns["CategoryID"].Visible = false;

                    if (dgvCategories.Columns.Contains("CategoryName"))
                        dgvCategories.Columns["CategoryName"].HeaderText = "Название категории";

                    if (dgvCategories.Columns.Contains("Description"))
                        dgvCategories.Columns["Description"].HeaderText = "Описание";

                    // Добавляем столбец с кнопками, если его еще нет
                    if (!dgvCategories.Columns.Contains("Actions"))
                    {
                        DataGridViewButtonColumn editButtonColumn = new DataGridViewButtonColumn();
                        editButtonColumn.Name = "Actions";
                        editButtonColumn.HeaderText = "Действия";
                        editButtonColumn.Text = "Изменить";
                        editButtonColumn.UseColumnTextForButtonValue = true;
                        dgvCategories.Columns.Add(editButtonColumn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtCategoryName.Text = "";
            txtDescription.Text = "";
            currentCategoryId = 0;
            btnSave.Text = "Добавить категорию";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCategoryName.Text))
            {
                MessageBox.Show("Пожалуйста, введите название категории", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (currentCategoryId == 0)
                {
                    // Добавляем новую категорию
                    string insertQuery = @"
                        INSERT INTO Categories (CategoryName, Description)
                        VALUES (@CategoryName, @Description);
                        SELECT SCOPE_IDENTITY();";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CategoryName", txtCategoryName.Text),
                        new SqlParameter("@Description", txtDescription.Text)
                    };

                    object result = DatabaseHelper.ExecuteScalar(insertQuery, parameters);
                    if (result != null)
                    {
                        MessageBox.Show("Категория успешно добавлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // Обновляем существующую категорию
                    string updateQuery = @"
                        UPDATE Categories 
                        SET CategoryName = @CategoryName, 
                            Description = @Description
                        WHERE CategoryID = @CategoryID";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CategoryID", currentCategoryId),
                        new SqlParameter("@CategoryName", txtCategoryName.Text),
                        new SqlParameter("@Description", txtDescription.Text)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(updateQuery, parameters);
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Категория успешно обновлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                ClearFields();
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения категории: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentCategoryId == 0)
            {
                MessageBox.Show("Пожалуйста, выберите категорию для удаления", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Проверяем, используется ли категория в товарах
                string checkProductsQuery = "SELECT COUNT(*) FROM Products WHERE CategoryID = @CategoryID";
                SqlParameter[] checkParams = new SqlParameter[]
                {
                    new SqlParameter("@CategoryID", currentCategoryId)
                };

                int productCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkProductsQuery, checkParams));
                if (productCount > 0)
                {
                    MessageBox.Show("Невозможно удалить категорию, так как она используется в товарах", "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите удалить эту категорию?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string deleteQuery = "DELETE FROM Categories WHERE CategoryID = @CategoryID";
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CategoryID", currentCategoryId)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(deleteQuery, parameters);
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Категория успешно удалена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления категории: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvCategories_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvCategories.Columns.Contains("Actions"))
            {
                if (e.ColumnIndex == dgvCategories.Columns["Actions"].Index)
                {
                    try
                    {
                        // Нажата кнопка редактирования
                        DataGridViewRow row = dgvCategories.Rows[e.RowIndex];
                        currentCategoryId = Convert.ToInt32(row.Cells["CategoryID"].Value);
                        txtCategoryName.Text = row.Cells["CategoryName"].Value.ToString();
                        txtDescription.Text = row.Cells["Description"].Value != DBNull.Value ? row.Cells["Description"].Value.ToString() : "";

                        btnSave.Text = "Обновить категорию";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка выбора категории: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

       
    }
}