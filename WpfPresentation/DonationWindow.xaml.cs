using System;
using System.Diagnostics;
using System.Windows;

namespace WpfPresentation
{
    public partial class DonationWindow : Window
    {
        private const string DONATION_URL = "https://www.patriotthanks.com/donate";

        public DonationWindow()
        {
            InitializeComponent();
        }
        private void btnDonateOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open URL in default browser
                var processInfo = new ProcessStartInfo
                {
                    FileName = DONATION_URL,
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open browser. Please visit {DONATION_URL} directly.\n\nError: {ex.Message}",
                    "Browser Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}