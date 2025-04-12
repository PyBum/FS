using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FlowerShopManagement
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
            Text = "Панель управления";
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Получаем общее количество товаров
                string productQuery = "SELECT COUNT(*) FROM Products";
                object productCount = DatabaseHelper.ExecuteScalar(productQuery);
                lblProductCount.Text = productCount?.ToString() ?? "0";


                // Получаем общее количество заказов
                string orderQuery = "SELECT COUNT(*) FROM Orders";
                object orderCount = DatabaseHelper.ExecuteScalar(orderQuery);
                lblOrderCount.Text = orderCount?.ToString() ?? "0";

                // Получаем общее количество клиентов
                string customerQuery = "SELECT COUNT(*) FROM Users WHERE Role = 'Customer'";
                object customerCount = DatabaseHelper.ExecuteScalar(customerQuery);
                lblCustomerCount.Text = customerCount?.ToString() ?? "0";

                // Получаем товары с низким запасом
                string lowStockQuery = @"
                    SELECT TOP 5 ProductName, QuantityInStock 
                    FROM Products 
                    WHERE QuantityInStock <= 10 
                    ORDER BY QuantityInStock ASC";
                DataTable lowStockProducts = DatabaseHelper.ExecuteQuery(lowStockQuery);
                dgvLowStock.DataSource = lowStockProducts;

                // Получаем последние заказы
                string recentOrdersQuery = @"
                    SELECT TOP 5 o.OrderID, u.FirstName + ' ' + u.LastName AS CustomerName, 
                           o.OrderDate, o.TotalAmount, o.Status
                    FROM Orders o
                    INNER JOIN Users u ON o.UserID = u.UserID
                    ORDER BY o.OrderDate DESC";
                DataTable recentOrders = DatabaseHelper.ExecuteQuery(recentOrdersQuery);
                dgvRecentOrders.DataSource = recentOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}