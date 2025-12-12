using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfPresentation
{
    public partial class ContactUsWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // API endpoint - matches your Next.js route
        private const string API_URL = "https://www.patriotthanks.com/api/contact";

        public ContactUsWindow()
        {
            InitializeComponent();
            txtMessage.TextChanged += TxtMessage_TextChanged;

            // Set default selections
            cboCategory.SelectedIndex = 0;
            cboUrgency.SelectedIndex = 2; // Normal
        }

        private void TxtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            int charCount = txtMessage.Text.Length;
            lblCharCount.Text = $"{charCount}/2000 characters";

            // Change color when approaching limit
            if (charCount > 1800)
            {
                lblCharCount.Foreground = new SolidColorBrush(Colors.OrangeRed);
            }
            else
            {
                lblCharCount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            }
        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous status
            lblStatus.Visibility = Visibility.Collapsed;

            // Validate inputs
            if (!ValidateForm())
            {
                return;
            }

            // Disable submit button and show loading state
            btnSubmit.IsEnabled = false;
            btnSubmit.Content = "Sending...";

            try
            {
                var result = await SubmitContactFormAsync();

                if (result.Success)
                {
                    ShowStatus($"Message sent successfully! Expected response: {result.ExpectedResponse}", true);

                    // Clear form after successful submission
                    ClearForm();

                    // Close window after a brief delay
                    await Task.Delay(2000);

                    MessageBox.Show(
                        $"Thank you for your message!\n\n" +
                        $"We will respond to your inquiry within {result.ExpectedResponse}.\n\n" +
                        "You should receive a confirmation email shortly.",
                        "Message Sent",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.Close();
                }
                else
                {
                    ShowStatus($"Failed to send message: {result.ErrorMessage}\n\nPlease try again or contact us directly at privacy@patriotthanks.com", false);
                }
            }
            catch (HttpRequestException)
            {
                ShowStatus("Unable to connect to the server. Please check your internet connection or contact us directly at privacy@patriotthanks.com", false);
            }
            catch (TaskCanceledException)
            {
                ShowStatus("Request timed out. Please try again or contact us directly at privacy@patriotthanks.com", false);
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}\n\nPlease contact us directly at privacy@patriotthanks.com", false);
            }
            finally
            {
                // Re-enable submit button
                btnSubmit.IsEnabled = true;
                btnSubmit.Content = "Send Message";
            }
        }

        private bool ValidateForm()
        {
            // Validate Email (Required)
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowValidationError("Email address is required.");
                txtEmail.Focus();
                return false;
            }

            // Validate Email Format
            string emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            if (!Regex.IsMatch(txtEmail.Text.Trim(), emailPattern))
            {
                ShowValidationError("Please enter a valid email address.");
                txtEmail.Focus();
                return false;
            }

            // Validate Category (Required)
            if (cboCategory.SelectedItem == null)
            {
                ShowValidationError("Please select a category.");
                cboCategory.Focus();
                return false;
            }

            // Validate Subject (Required)
            if (string.IsNullOrWhiteSpace(txtSubject.Text))
            {
                ShowValidationError("Subject is required.");
                txtSubject.Focus();
                return false;
            }

            // Validate Message (Required)
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                ShowValidationError("Message is required.");
                txtMessage.Focus();
                return false;
            }

            if (txtMessage.Text.Trim().Length < 10)
            {
                ShowValidationError("Message must be at least 10 characters long.");
                txtMessage.Focus();
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            lblStatus.Text = message;
            lblStatus.Foreground = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#155724"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721c24"));
            lblStatus.Visibility = Visibility.Visible;
        }

        private async Task<ContactResult> SubmitContactFormAsync()
        {
            // Get category tag value
            string categoryValue = "general-inquiry";
            if (cboCategory.SelectedItem is ComboBoxItem categoryItem && categoryItem.Tag != null)
            {
                categoryValue = categoryItem.Tag.ToString();
            }

            // Get urgency tag value
            string urgencyValue = "normal";
            if (cboUrgency.SelectedItem is ComboBoxItem urgencyItem && urgencyItem.Tag != null)
            {
                urgencyValue = urgencyItem.Tag.ToString();
            }

            // Build form data matching the API schema
            var formData = new
            {
                firstname = txtFirstName.Text.Trim(),
                lastname = txtLastName.Text.Trim(),
                email = txtEmail.Text.Trim(),
                subject = txtSubject.Text.Trim(),
                category = categoryValue,      // Required by API
                urgency = urgencyValue,
                message = txtMessage.Text.Trim(),
                userTier = "free",             // Default tier for desktop app users
                source = "WPF Desktop Application"
            };

            var jsonContent = JsonSerializer.Serialize(formData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Set a reasonable timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await _httpClient.PostAsync(API_URL, content);

            if (response.IsSuccessStatusCode)
            {
                // Parse response to get expected response time
                var responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    var expectedResponse = doc.RootElement.TryGetProperty("expectedResponse", out var prop)
                        ? prop.GetString()
                        : "1-2 business days";

                    return new ContactResult
                    {
                        Success = true,
                        ExpectedResponse = expectedResponse
                    };
                }
                catch
                {
                    return new ContactResult
                    {
                        Success = true,
                        ExpectedResponse = "1-2 business days"
                    };
                }
            }
            else
            {
                // Try to get error message from response
                var errorBody = await response.Content.ReadAsStringAsync();
                string errorMessage = "Unknown error";

                try
                {
                    using var doc = JsonDocument.Parse(errorBody);
                    if (doc.RootElement.TryGetProperty("message", out var msgProp))
                    {
                        errorMessage = msgProp.GetString();
                    }
                }
                catch
                {
                    errorMessage = $"Server returned status {(int)response.StatusCode}";
                }

                return new ContactResult
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        private void ClearForm()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            cboCategory.SelectedIndex = 0;
            txtSubject.Clear();
            cboUrgency.SelectedIndex = 2;
            txtMessage.Clear();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Check if form has data
            if (!string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                !string.IsNullOrWhiteSpace(txtLastName.Text) ||
                !string.IsNullOrWhiteSpace(txtEmail.Text) ||
                !string.IsNullOrWhiteSpace(txtSubject.Text) ||
                !string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                var result = MessageBox.Show(
                    "You have unsaved information. Are you sure you want to close?",
                    "Confirm Close",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            this.Close();
        }

        private class ContactResult
        {
            public bool Success { get; set; }
            public string ExpectedResponse { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}