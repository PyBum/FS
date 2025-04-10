using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FlowerShopManagement
{
    public partial class OrdersForm : Form
    {
        public OrdersForm()
        {
            InitializeComponent();
            Text = "Управление заказами";
        }

        private void OrdersForm_Load(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"
                    SELECT o.OrderID, u.FirstName + ' ' + u.LastName AS CustomerName, 
                           o.OrderDate, o.TotalAmount, o.Status, o.DeliveryDate
                    FROM Orders o
                    INNER JOIN Users u ON o.UserID = u.UserID
                    ORDER BY o.OrderDate DESC";

                DataTable orders = DatabaseHelper.ExecuteQuery(query);
                dgvOrders.DataSource = orders;

                // Настраиваем столбцы
                if (dgvOrders.Columns.Contains("OrderID"))
                    dgvOrders.Columns["OrderID"].Visible = false;

                if (dgvOrders.Columns.Contains("CustomerName"))
                    dgvOrders.Columns["CustomerName"].HeaderText = "Клиент";

                if (dgvOrders.Columns.Contains("OrderDate"))
                    dgvOrders.Columns["OrderDate"].HeaderText = "Дата заказа";

                if (dgvOrders.Columns.Contains("TotalAmount"))
                    dgvOrders.Columns["TotalAmount"].HeaderText = "Сумма";

                if (dgvOrders.Columns.Contains("Status"))
                    dgvOrders.Columns["Status"].HeaderText = "Статус";

                if (dgvOrders.Columns.Contains("DeliveryDate"))
                    dgvOrders.Columns["DeliveryDate"].HeaderText = "Дата доставки";

                // Добавляем столбец с кнопками
                if (!dgvOrders.Columns.Contains("Actions"))
                {
                    DataGridViewButtonColumn viewButtonColumn = new DataGridViewButtonColumn();
                    viewButtonColumn.Name = "Actions";
                    viewButtonColumn.HeaderText = "Действия";
                    viewButtonColumn.Text = "Просмотр";
                    viewButtonColumn.UseColumnTextForButtonValue = true;
                    dgvOrders.Columns.Add(viewButtonColumn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrders.Columns.Contains("Actions"))
            {
                if (e.ColumnIndex == dgvOrders.Columns["Actions"].Index)
                {
                    try
                    {
                        // Нажата кнопка просмотра деталей
                        int orderId = Convert.ToInt32(dgvOrders.Rows[e.RowIndex].Cells["OrderID"].Value);
                        using (OrderDetailsForm detailsForm = new OrderDetailsForm(orderId))
                        {
                            detailsForm.ShowDialog();
                        }

                        // Обновляем список заказов после закрытия формы деталей
                        LoadOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка просмотра деталей заказа: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    // Заглушка для формы деталей заказа
    public class OrderDetailsForm : Form
    {
        private readonly int orderId;
        private Label lblOrderId;
        private System.ComponentModel.IContainer components = null;

        public OrderDetailsForm(int orderId)
        {
            this.orderId = orderId;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Детали заказа - Заказ #" + orderId;
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.White;

            lblOrderId = new Label();
            lblOrderId.Text = "ID заказа: " + orderId;
            lblOrderId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            lblOrderId.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblOrderId.Location = new System.Drawing.Point(20, 20);
            lblOrderId.AutoSize = true;

            this.Controls.Add(lblOrderId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}