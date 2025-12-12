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
using LogicLayerInterfaces;
using DataDomain;

namespace WpfPresentation
{
    /// <summary>
    /// Interaction logic for AddEditUserWindow.xaml
    /// </summary>
    public partial class AddEditUserWindow : Window
    {
        // Enum to track window mode
        public enum WindowMode { Add, Edit, View }

        private IUserManager _userManager;
        private ILookupManager _lookupManager;
        private User _user;
        private User _originalUser;  // Keep original for concurrency check
        private WindowMode _mode;
        private bool _originalAccountLocked;  // Track original lock status

        // Constructor for Add mode - only needs managers
        public AddEditUserWindow(IUserManager userManager, ILookupManager lookupManager)
        {
            _userManager = userManager;
            _lookupManager = lookupManager;
            _user = new User();
            _originalUser = null;
            _mode = WindowMode.Add;

            InitializeComponent();
        }

        // Constructor for Edit mode - needs managers, user, and editMode flag
        public AddEditUserWindow(IUserManager userManager, ILookupManager lookupManager, User user, bool editMode)
        {
            _userManager = userManager;
            _lookupManager = lookupManager;
            _user = user;
            _originalUser = user;  // Store original for concurrency
            _mode = editMode ? WindowMode.Edit : WindowMode.View;

            InitializeComponent();
        }

        // Constructor for View/Detail mode (default when user passed without editMode flag)
        public AddEditUserWindow(IUserManager userManager, ILookupManager lookupManager, User user)
            : this(userManager, lookupManager, user, false)  // Default to View mode
        {
        }

        private bool ShowValidationError(string message, Control focusControl)
        {
            MessageBox.Show(message,
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            focusControl.Focus();
            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load combo box data
            LoadComboBoxData();

            switch (_mode)
            {
                case WindowMode.Add:
                    lblTitle.Content = "Add a New User";
                    btnAction.Content = "Save";
                    btnCancel.Content = "Cancel";
                    SetFieldsReadOnly(false);

                    // Select "Please Select" (index 0) for all combo boxes
                    if (cmbTitle.Items.Count > 0)
                        cmbTitle.SelectedIndex = 0;
                    if (cmbStatus.Items.Count > 0)
                        cmbStatus.SelectedIndex = 0;
                    if (cmbAccountStatus.Items.Count > 0)
                        cmbAccountStatus.SelectedIndex = 0;
                    if (cmbMemLevel.Items.Count > 0)
                        cmbMemLevel.SelectedIndex = 0;
                    break;

                case WindowMode.Edit:
                    lblTitle.Content = "Edit User";
                    btnAction.Content = "Save";
                    btnCancel.Content = "Cancel";
                    SetFieldsReadOnly(false);
                    PopulateFieldsFromUser();
                    ShowLockControls(true);
                    txtFirstName.Focus();
                    break;

                case WindowMode.View:
                    lblTitle.Content = "View User Details";
                    btnAction.Content = "Edit";
                    btnCancel.Content = "OK";
                    SetFieldsReadOnly(true);
                    PopulateFieldsFromUser();
                    ShowLockControls(true);
                    chkAccountLocked.IsEnabled = false;  // Read-only in View mode
                    break;
            }
        }

        private void PopulateFieldsFromUser()
        {
            // Set combo box selections
            cmbTitle.SelectedItem = _user.TitleID;
            txtFirstName.Text = _user.FirstName;
            txtLastName.Text = _user.LastName;
            txtEmail.Text = _user.Email;
            cmbStatus.SelectedItem = _user.StatusID;
            cmbAccountStatus.SelectedItem = _user.AccountStatusID;
            cmbMemLevel.SelectedItem = _user.MemLevelID;

            // Set account locked status
            _originalAccountLocked = _user.AccountLocked;
            chkAccountLocked.IsChecked = _user.AccountLocked;
            UpdateLockStatusDisplay();
        }

        private void UpdateLockStatusDisplay()
        {
            if (chkAccountLocked.IsChecked == true)
            {
                txtLockStatus.Text = "🔒 Account is LOCKED - user cannot log in";
                txtLockStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                txtLockStatus.Text = "🔓 Account is unlocked";
                txtLockStatus.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Load Titles - add "Please Select" option
                var titles = _lookupManager.GetAllTitles();
                var titleList = new List<string> { "-- Please Select --" };
                titleList.AddRange(titles.Select(t => t.TitleID));
                cmbTitle.ItemsSource = titleList;

                // Load Status - add "Please Select" option
                var statuses = _lookupManager.GetAllStatuses();
                var statusList = new List<string> { "-- Please Select --" };
                statusList.AddRange(statuses.Select(s => s.StatusID));
                cmbStatus.ItemsSource = statusList;

                // Load Account Status - add "Please Select" option
                var accountStatuses = _lookupManager.GetAllAccountStatuses();
                var accountStatusList = new List<string> { "-- Please Select --" };
                accountStatusList.AddRange(accountStatuses.Select(a => a.AccountStatusID));
                cmbAccountStatus.ItemsSource = accountStatusList;

                // Load Membership Levels - ONLY ACTIVE ones, add "Please Select" option
                var memLevels = _lookupManager.GetActiveMembershipLevels();
                var memLevelList = new List<string> { "-- Please Select --" };
                memLevelList.AddRange(memLevels.Select(m => m.MemLevelID));
                cmbMemLevel.ItemsSource = memLevelList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form data: " + ex.Message,
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetFieldsReadOnly(bool isReadOnly)
        {
            cmbTitle.IsEnabled = !isReadOnly;
            txtFirstName.IsReadOnly = isReadOnly;
            txtLastName.IsReadOnly = isReadOnly;
            txtEmail.IsReadOnly = isReadOnly;
            cmbStatus.IsEnabled = !isReadOnly;
            cmbAccountStatus.IsEnabled = !isReadOnly;
            cmbMemLevel.IsEnabled = !isReadOnly;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            switch (_mode)
            {
                case WindowMode.View:
                    // Switch from View to Edit mode
                    _mode = WindowMode.Edit;
                    lblTitle.Content = "Edit User";
                    SetFieldsReadOnly(false);
                    chkAccountLocked.IsEnabled = true;  // Enable lock checkbox in Edit mode
                    btnAction.Content = "Save";
                    btnCancel.Content = "Cancel";
                    txtFirstName.SelectAll();
                    txtFirstName.Focus();
                    break;

                case WindowMode.Edit:
                    // Save changes to existing user
                    if (!ValidateInput())
                    {
                        return;
                    }

                    // Create new user object with updated values
                    User updatedUser = new User()
                    {
                        UserID = _user.UserID,  // Keep the same ID
                        TitleID = cmbTitle.SelectedItem.ToString(),
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        StatusID = cmbStatus.SelectedItem.ToString(),
                        AccountStatusID = cmbAccountStatus.SelectedItem.ToString(),
                        MemLevelID = cmbMemLevel.SelectedItem.ToString()
                    };

                    try
                    {
                        bool result = _userManager.EditUser(updatedUser, _originalUser);
                        if (result)
                        {
                            this.DialogResult = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ex.Message;
                        if (ex.InnerException != null)
                        {
                            errorMessage += "\n\n" + ex.InnerException.Message;
                        }
                        MessageBox.Show(errorMessage,
                            "Update Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    break;

                case WindowMode.Add:
                    // Save new user
                    if (!ValidateInput())
                    {
                        return;
                    }

                    // Create new user object for insert
                    User newUser = new User()
                    {
                        TitleID = cmbTitle.SelectedItem.ToString(),
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        StatusID = cmbStatus.SelectedItem.ToString(),
                        AccountStatusID = cmbAccountStatus.SelectedItem.ToString(),
                        MemLevelID = cmbMemLevel.SelectedItem.ToString()
                    };

                    try
                    {
                        int newUserID = _userManager.AddUser(newUser);
                        if (newUserID > 0)
                        {
                            MessageBox.Show($"User added successfully with ID: {newUserID}",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            this.DialogResult = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ex.Message;
                        if (ex.InnerException != null)
                        {
                            errorMessage += "\n\n" + ex.InnerException.Message;
                        }
                        MessageBox.Show(errorMessage,
                            "Add Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    break;
            }
        }

        private bool ValidateInput()
        {
            // Validate Title selection
            if (cmbTitle.SelectedItem == null || cmbTitle.SelectedItem.ToString() == "-- Please Select --")
                return ShowValidationError("Please select a title.", cmbTitle);

            // Validate First Name
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                return ShowValidationError("First name is required.", txtFirstName);

            if (txtFirstName.Text.Trim().Length > 256)
                return ShowValidationError("First name cannot exceed 256 characters.", txtFirstName);

            // Validate Last Name
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
                return ShowValidationError("Last name is required.", txtLastName);

            if (txtLastName.Text.Trim().Length > 256)
                return ShowValidationError("Last name cannot exceed 256 characters.", txtLastName);

            // Validate Email
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
                return ShowValidationError("Email address is required.", txtEmail);

            if (txtEmail.Text.Trim().Length > 256)
                return ShowValidationError("Email address cannot exceed 256 characters.", txtEmail);

            // Basic email format validation
            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
                return ShowValidationError("Please enter a valid email address.", txtEmail);

            // Validate Status selection
            if (cmbStatus.SelectedItem == null || cmbStatus.SelectedItem.ToString() == "-- Please Select --")
                return ShowValidationError("Please select a status.", cmbStatus);

            // Validate Account Status selection
            if (cmbAccountStatus.SelectedItem == null || cmbAccountStatus.SelectedItem.ToString() == "-- Please Select --")
                return ShowValidationError("Please select an account status.", cmbAccountStatus);

            // Validate Membership Level selection
            if (cmbMemLevel.SelectedItem == null || cmbMemLevel.SelectedItem.ToString() == "-- Please Select --")
                return ShowValidationError("Please select a membership level.", cmbMemLevel);

            return true;
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void ShowLockControls(bool show)
        {
            var visibility = show ? Visibility.Visible : Visibility.Collapsed;
            lblAccountLocked.Visibility = visibility;
            chkAccountLocked.Visibility = visibility;
            txtLockStatus.Visibility = visibility;
        }

        private void chkAccountLocked_Click(object sender, RoutedEventArgs e)
        {
            bool newLockStatus = chkAccountLocked.IsChecked == true;
            string action = newLockStatus ? "lock" : "unlock";

            var result = MessageBox.Show(
                $"Are you sure you want to {action} this account?\n\n" +
                (newLockStatus
                    ? "The user will NOT be able to log in until the account is unlocked."
                    : "The user will be able to log in again."),
                $"Confirm Account {(newLockStatus ? "Lock" : "Unlock")}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _userManager.SetAccountLocked(_user.Email, newLockStatus);
                    if (success)
                    {
                        _user.AccountLocked = newLockStatus;
                        UpdateLockStatusDisplay();
                        MessageBox.Show(
                            $"Account has been {(newLockStatus ? "locked" : "unlocked")} successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    // Revert checkbox to previous state
                    chkAccountLocked.IsChecked = !newLockStatus;
                    MessageBox.Show(
                        $"Failed to {action} account: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                // User cancelled - revert checkbox
                chkAccountLocked.IsChecked = !newLockStatus;
            }
        }
    }
}