using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DataDomain;
using LogicLayerInterfaces;

namespace WpfPresentation
{
    /// <summary>
    /// Interaction logic for UpdatePasswordWindow.xaml
    /// </summary>
    public partial class UpdatePasswordWindow : Window
    {
        private IUserManager _userManager;
        private UserVM _accessToken;
        private bool _forced = false;

        public UpdatePasswordWindow(IUserManager userManager, UserVM accessToken, bool forced=false)
        {
            _userManager = userManager;
            _accessToken = accessToken;
            _forced = forced;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_forced)
            {
                txtEmail.Text = _accessToken.Email;
                txtEmail.IsEnabled = false;
                btnCancel.Visibility = Visibility.Collapsed;
                pwdCurrentPassword.Focus();
            }
            else
            {
                txtEmail.Focus();
            }
            btnSubmit.IsDefault = true;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string currentPassword = pwdCurrentPassword.Password;
            string newPassword = pwdNewPassword.Password;
            string retypePassword = pwdRetypePassword.Password;

            if (email != _accessToken.Email || email.Length < 10 || email.Length > 250)
            {
                MessageBox.Show("Invalid Email");
                txtEmail.Focus();
                txtEmail.SelectAll();
                return;
            }

            if (currentPassword.Length < 6 || currentPassword.Length > 250)
            {
                MessageBox.Show("Invalid Password");
                pwdCurrentPassword.Focus();
                pwdCurrentPassword.Clear();
                return;
            }

            if (newPassword != retypePassword)
            {
                MessageBox.Show("Passwords must match");
                pwdNewPassword.Clear();
                pwdRetypePassword.Clear();
                pwdNewPassword.Focus();
                return;
            }

            try
            {
                if (_userManager.ResetPassword(email, currentPassword, newPassword))
                {
                    MessageBox.Show("Password Updated.");
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password update failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                string errorMessage = "Update Password Failed!\n\n" + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nDetails: " + ex.InnerException.Message;
                }

                MessageBox.Show(errorMessage);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If forced mode and password wasn't updated, prevent closing
            if (_forced && this.DialogResult != true)
            {
                var result = MessageBox.Show(
                    "You must update your password before continuing. Do you want to close the application?",
                    "Password Update Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true; // Prevent window from closing
                }
                else
                {
                    // User chose to quit - exit the application
                    Application.Current.Shutdown();
                }
            }
        }


    }
}
