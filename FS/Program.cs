using System;
using System.Windows.Forms;

namespace FlowerShopManagement
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            
            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
               
                Application.Run(new MainForm(loginForm.CurrentUser));
            }
            else
            {
                
                Application.Exit();
            }
        }
    }
}