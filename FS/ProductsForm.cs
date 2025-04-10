using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlowerShopManagement
{
    public partial class ProductsForm : Form
    {
        private int currentProductId = 0;
        private string imagePath = "";

        public ProductsForm()
        {
            InitializeComponent();
            Text = "Управление товарами";
        }

        private void ProductsForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadProducts();
            ClearFields();
        }

        private void LoadCategories()
        {
            try
            {
                string query = "SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName";
                DataTable categories = DatabaseHelper.ExecuteQuery(query);

                if (categories != null && categories.Rows.Count > 0)
                {
                    cmbCategory.DataSource = categories;
                    cmbCategory.DisplayMember = "CategoryName";
                    cmbCategory.ValueMember = "CategoryID";
                    cmbCategory.SelectedIndex = -1;

                    // Для фильтрации
                    DataTable filterCategories = categories.Copy();
                    DataRow emptyRow = filterCategories.NewRow();
                    emptyRow["CategoryID"] = DBNull.Value;
                    emptyRow["CategoryName"] = "-- Все категории --";
                    filterCategories.Rows.InsertAt(emptyRow, 0);

                    cmbFilterCategory.DataSource = filterCategories;
                    cmbFilterCategory.DisplayMember = "CategoryName";
                    cmbFilterCategory.ValueMember = "CategoryID";
                    cmbFilterCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts(string searchTerm = "", int? categoryId = null)
        {
            try
            {
                string query = @"
                    SELECT p.ProductID, p.ProductName, p.Description, p.Price, p.QuantityInStock, 
                           c.CategoryName, p.CategoryID,
                           (SELECT TOP 1 ImagePath FROM ProductImages WHERE ProductID = p.ProductID AND IsPrimary = 1) AS PrimaryImage
                    FROM Products p
                    INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                    WHERE (@SearchTerm = '' OR p.ProductName LIKE '%' + @SearchTerm + '%' OR p.Description LIKE '%' + @SearchTerm + '%')
                    AND (@CategoryID IS NULL OR p.CategoryID = @CategoryID)
                    ORDER BY p.ProductName";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@SearchTerm", searchTerm ?? string.Empty),
                    new SqlParameter("@CategoryID", (object)categoryId ?? DBNull.Value)
                };

                DataTable products = DatabaseHelper.ExecuteQuery(query, parameters);

                if (products != null)
                {
                    dgvProducts.DataSource = products;

                    // Настраиваем столбцы
                    if (dgvProducts.Columns.Contains("ProductID"))
                        dgvProducts.Columns["ProductID"].Visible = false;

                    if (dgvProducts.Columns.Contains("CategoryID"))
                        dgvProducts.Columns["CategoryID"].Visible = false;

                    if (dgvProducts.Columns.Contains("PrimaryImage"))
                        dgvProducts.Columns["PrimaryImage"].Visible = false;

                    if (dgvProducts.Columns.Contains("ProductName"))
                        dgvProducts.Columns["ProductName"].HeaderText = "Название товара";

                    if (dgvProducts.Columns.Contains("Description"))
                        dgvProducts.Columns["Description"].HeaderText = "Описание";

                    if (dgvProducts.Columns.Contains("Price"))
                        dgvProducts.Columns["Price"].HeaderText = "Цена";

                    if (dgvProducts.Columns.Contains("QuantityInStock"))
                        dgvProducts.Columns["QuantityInStock"].HeaderText = "Запас";

                    if (dgvProducts.Columns.Contains("CategoryName"))
                        dgvProducts.Columns["CategoryName"].HeaderText = "Категория";

                    // Добавляем столбец с кнопками, если его еще нет
                    if (!dgvProducts.Columns.Contains("Actions"))
                    {
                        DataGridViewButtonColumn editButtonColumn = new DataGridViewButtonColumn();
                        editButtonColumn.Name = "Actions";
                        editButtonColumn.HeaderText = "Действия";
                        editButtonColumn.Text = "Изменить";
                        editButtonColumn.UseColumnTextForButtonValue = true;
                        dgvProducts.Columns.Add(editButtonColumn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtProductName.Text = "";
            txtDescription.Text = "";
            txtPrice.Text = "";
            txtQuantity.Text = "";

            if (cmbCategory.Items.Count > 0)
                cmbCategory.SelectedIndex = -1;

            currentProductId = 0;
            btnSave.Text = "Добавить товар";
            picProductImage.Image = null;
            imagePath = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductName.Text) ||
                string.IsNullOrEmpty(txtPrice.Text) ||
                string.IsNullOrEmpty(txtQuantity.Text) ||
                cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную цену", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное количество", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (currentProductId == 0)
                {
                    // Добавляем новый товар
                    string insertQuery = @"
                        INSERT INTO Products (ProductName, Description, Price, QuantityInStock, CategoryID, CreatedDate, ModifiedDate)
                        VALUES (@ProductName, @Description, @Price, @QuantityInStock, @CategoryID, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ProductName", txtProductName.Text),
                        new SqlParameter("@Description", txtDescription.Text),
                        new SqlParameter("@Price", price),
                        new SqlParameter("@QuantityInStock", quantity),
                        new SqlParameter("@CategoryID", cmbCategory.SelectedValue)
                    };

                    object result = DatabaseHelper.ExecuteScalar(insertQuery, parameters);
                    if (result != null)
                    {
                        int newProductId = Convert.ToInt32(result);

                        // Сохраняем изображение, если оно выбрано
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            SaveProductImage(newProductId, imagePath, true);
                        }

                        MessageBox.Show("Товар успешно добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // Обновляем существующий товар
                    string updateQuery = @"
                        UPDATE Products 
                        SET ProductName = @ProductName, 
                            Description = @Description, 
                            Price = @Price, 
                            QuantityInStock = @QuantityInStock, 
                            CategoryID = @CategoryID,
                            ModifiedDate = GETDATE()
                        WHERE ProductID = @ProductID";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ProductID", currentProductId),
                        new SqlParameter("@ProductName", txtProductName.Text),
                        new SqlParameter("@Description", txtDescription.Text),
                        new SqlParameter("@Price", price),
                        new SqlParameter("@QuantityInStock", quantity),
                        new SqlParameter("@CategoryID", cmbCategory.SelectedValue)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(updateQuery, parameters);
                    if (rowsAffected > 0)
                    {
                        // Сохраняем изображение, если оно выбрано
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            SaveProductImage(currentProductId, imagePath, true);
                        }

                        MessageBox.Show("Товар успешно обновлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                ClearFields();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения товара: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveProductImage(int productId, string imagePath, bool isPrimary)
        {
            try
            {
                // Если устанавливаем как основное, обновляем все существующие изображения на неосновные
                if (isPrimary)
                {
                    string updateQuery = "UPDATE ProductImages SET IsPrimary = 0 WHERE ProductID = @ProductID";
                    SqlParameter[] updateParams = new SqlParameter[]
                    {
                        new SqlParameter("@ProductID", productId)
                    };
                    DatabaseHelper.ExecuteNonQuery(updateQuery, updateParams);
                }

                // Сохраняем файл изображения в папку
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagePath);
                string targetDirectory = Path.Combine(Application.StartupPath, "ProductImages");

                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                string targetPath = Path.Combine(targetDirectory, fileName);
                File.Copy(imagePath, targetPath, true);

                // Сохраняем информацию об изображении в базу данных
                string insertQuery = @"
                    INSERT INTO ProductImages (ProductID, ImagePath, IsPrimary, UploadDate)
                    VALUES (@ProductID, @ImagePath, @IsPrimary, GETDATE())";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ProductID", productId),
                    new SqlParameter("@ImagePath", fileName),
                    new SqlParameter("@IsPrimary", isPrimary)
                };

                DatabaseHelper.ExecuteNonQuery(insertQuery, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения изображения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Пожалуйста, выберите товар для удаления", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Проверяем, используется ли товар в заказах
                    string checkOrdersQuery = "SELECT COUNT(*) FROM OrderDetails WHERE ProductID = @ProductID";
                    SqlParameter[] checkParams = new SqlParameter[]
                    {
                        new SqlParameter("@ProductID", currentProductId)
                    };

                    int orderCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkOrdersQuery, checkParams));
                    if (orderCount > 0)
                    {
                        MessageBox.Show("Невозможно удалить товар, так как он используется в заказах", "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Сначала удаляем изображения товара
                    string deleteImagesQuery = "DELETE FROM ProductImages WHERE ProductID = @ProductID";
                    SqlParameter[] imageParams = new SqlParameter[]
                    {
                        new SqlParameter("@ProductID", currentProductId)
                    };
                    DatabaseHelper.ExecuteNonQuery(deleteImagesQuery, imageParams);

                    // Затем удаляем сам товар
                    string deleteQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ProductID", currentProductId)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(deleteQuery, parameters);
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Товар успешно удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadProducts();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления товара: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvProducts.Columns.Contains("Actions"))
            {
                if (e.ColumnIndex == dgvProducts.Columns["Actions"].Index)
                {
                    try
                    {
                        // Нажата кнопка редактирования
                        DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
                        currentProductId = Convert.ToInt32(row.Cells["ProductID"].Value);
                        txtProductName.Text = row.Cells["ProductName"].Value.ToString();
                        txtDescription.Text = row.Cells["Description"].Value.ToString();
                        txtPrice.Text = row.Cells["Price"].Value.ToString();
                        txtQuantity.Text = row.Cells["QuantityInStock"].Value.ToString();

                        // Устанавливаем категорию
                        int categoryId = Convert.ToInt32(row.Cells["CategoryID"].Value);
                        cmbCategory.SelectedValue = categoryId;

                        // Загружаем основное изображение, если оно существует
                        if (row.Cells["PrimaryImage"].Value != DBNull.Value)
                        {
                            string imageName = row.Cells["PrimaryImage"].Value.ToString();
                            string imagePath = Path.Combine(Application.StartupPath, "ProductImages", imageName);
                            if (File.Exists(imagePath))
                            {
                                if (picProductImage.Image != null)
                                {
                                    picProductImage.Image.Dispose();
                                }
                                picProductImage.Image = Image.FromFile(imagePath);
                            }
                            else
                            {
                                picProductImage.Image = null;
                            }
                        }
                        else
                        {
                            picProductImage.Image = null;
                        }

                        btnSave.Text = "Обновить товар";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка выбора товара: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Файлы изображений|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Выберите изображение товара";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    imagePath = openFileDialog.FileName;
                    try
                    {
                        if (picProductImage.Image != null)
                        {
                            picProductImage.Image.Dispose();
                        }
                        picProductImage.Image = Image.FromFile(imagePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка загрузки изображения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilterCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            try
            {
                string searchTerm = txtSearch.Text.Trim();
                int? categoryId = null;

                if (cmbFilterCategory.SelectedIndex > 0 && cmbFilterCategory.SelectedValue != DBNull.Value)
                {
                    categoryId = Convert.ToInt32(cmbFilterCategory.SelectedValue);
                }

                LoadProducts(searchTerm, categoryId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка применения фильтров: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel файлы|*.xlsx|CSV файлы|*.csv";
                saveFileDialog.Title = "Экспорт товаров";
                saveFileDialog.FileName = "Товары_Экспорт_" + DateTime.Now.ToString("yyyyMMdd");

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (saveFileDialog.FileName.EndsWith(".xlsx"))
                        {
                            ExportToExcel(saveFileDialog.FileName);
                        }
                        else if (saveFileDialog.FileName.EndsWith(".csv"))
                        {
                            ExportToCsv(saveFileDialog.FileName);
                        }

                        MessageBox.Show("Экспорт успешно завершен!", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка экспорта данных: " + ex.Message, "Ошибка экспорта", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToExcel(string fileName)
        {
            // В реальном приложении вы бы использовали библиотеку вроде EPPlus или NPOI
            MessageBox.Show("Функция экспорта в Excel будет реализована с использованием библиотеки EPPlus или NPOI", "Экспорт в Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportToCsv(string fileName)
        {
            try
            {
                // Создаем StringBuilder для хранения данных CSV
                System.Text.StringBuilder csv = new System.Text.StringBuilder();

                // Добавляем заголовки
                string[] headers = new string[dgvProducts.Columns.Count - 2]; // Исключаем столбцы ProductID и Actions
                int headerIndex = 0;

                for (int i = 0; i < dgvProducts.Columns.Count; i++)
                {
                    if (dgvProducts.Columns[i].Visible && dgvProducts.Columns[i].Name != "Actions")
                    {
                        headers[headerIndex++] = dgvProducts.Columns[i].HeaderText;
                    }
                }

                csv.AppendLine(string.Join(",", headers));

                // Добавляем строки
                foreach (DataGridViewRow row in dgvProducts.Rows)
                {
                    string[] fields = new string[headers.Length];
                    int fieldIndex = 0;

                    for (int i = 0; i < dgvProducts.Columns.Count; i++)
                    {
                        if (dgvProducts.Columns[i].Visible && dgvProducts.Columns[i].Name != "Actions")
                        {
                            string value = row.Cells[i].Value?.ToString() ?? "";
                            // Экранируем запятые и кавычки
                            if (value.Contains(",") || value.Contains("\""))
                            {
                                value = "\"" + value.Replace("\"", "\"\"") + "\"";
                            }
                            fields[fieldIndex++] = value;
                        }
                    }

                    csv.AppendLine(string.Join(",", fields));
                }

                // Записываем в файл
                System.IO.File.WriteAllText(fileName, csv.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка экспорта в CSV: " + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Освобождаем ресурсы изображения
                if (picProductImage.Image != null)
                {
                    picProductImage.Image.Dispose();
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}