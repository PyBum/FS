using System;
using System.Windows.Forms;

namespace FlowerShopManagement
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private Form activeForm = null;

        public MainForm(User user)
        {
            InitializeComponent();
            currentUser = user;
            Text = "Система управления цветочным магазином - " + currentUser.FullName;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Устанавливаем информацию о пользователе
            lblUser.Text = $"Пользователь: {currentUser.FullName} ({currentUser.Role})";

            // Настраиваем доступность кнопок в зависимости от роли пользователя
            SetButtonAvailability();

            // Открываем панель управления по умолчанию
            OpenChildForm(new DashboardForm());
        }

        private void SetButtonAvailability()
        {
            // Все пользователи могут видеть панель управления, товары и категории
            btnDashboard.Visible = true;
            btnProducts.Visible = true;
            btnCategories.Visible = true;
            btnOrders.Visible = true;

            // Только администраторы и менеджеры могут управлять поставщиками и инвентаризацией
            btnSuppliers.Visible = currentUser.IsAdmin || currentUser.IsManager;
            btnInventory.Visible = currentUser.IsAdmin || currentUser.IsManager;

            // Только администраторы могут управлять пользователями и видеть отчеты
            btnUsers.Visible = currentUser.IsAdmin;
            btnReports.Visible = currentUser.IsAdmin;
        }

        private void OpenChildForm(Form childForm)
        {
            if (activeForm != null)
            {
                activeForm.Close();
            }

            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            OpenChildForm(new DashboardForm());
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            OpenChildForm(new ProductsForm());
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            OpenChildForm(new CategoriesForm());
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            OpenChildForm(new OrdersForm());
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            if (currentUser.IsAdmin || currentUser.IsManager)
            {
                OpenChildForm(new SuppliersForm());
            }
            else
            {
                MessageBox.Show("У вас нет прав для доступа к этому разделу", "Ограничение доступа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            if (currentUser.IsAdmin || currentUser.IsManager)
            {
                OpenChildForm(new InventoryForm());
            }
            else
            {
                MessageBox.Show("У вас нет прав для доступа к этому разделу", "Ограничение доступа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            if (currentUser.IsAdmin)
            {
                OpenChildForm(new UsersForm());
            }
            else
            {
                MessageBox.Show("У вас нет прав для доступа к этому разделу", "Ограничение доступа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            if (currentUser.IsAdmin)
            {
                OpenChildForm(new ReportsForm());
            }
            else
            {
                MessageBox.Show("У вас нет прав для доступа к этому разделу", "Ограничение доступа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Система управления цветочным магазином\nВерсия 1.0\n\n© 2023 Все права защищены",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из программы?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}