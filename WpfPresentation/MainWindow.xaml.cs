using System;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataDomain;
using LogicLayer;
using LogicLayerInterfaces;

namespace WpfPresentation
{
    public partial class MainWindow : Window
    {
        private IUserManager _userManager;
        private UserVM _accessToken = null;
        private IBusinessManager _businessManager;
        private IIncentiveManager _incentiveManager;
        private ILookupManager _lookupManager;
        private Business _selectedBusinessForUpdate = null;
        private Business _selectedBusinessForIncentive = null;
        private List<string> _selectedIncentiveTypeIDs = new List<string>();
        private Incentive _selectedIncentiveForUpdate = null;
        private List<string> _selectedUpdateIncentiveTypeIDs = new List<string>();
        private bool _adminTabInitialized = false;

        public MainWindow()
        {
            _userManager = new UserManager();
            _businessManager = new BusinessManager();
            _lookupManager = new LookupManager();
            _incentiveManager = new IncentiveManager();

            InitializeComponent();

        }

        private void ShowValidationError(string message, Control focusControl = null)
        {
            MessageBox.Show(message,
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            focusControl?.Focus();
        }

        private void LoadIncentiveTypesToListBox(ListBox targetListBox, List<string> selectedTypesList)
        {
            try
            {
                if (targetListBox == null)
                {
                    return;
                }

                var incentiveTypes = _lookupManager.GetAllIncentiveTypes();
                targetListBox.ItemsSource = incentiveTypes;
                selectedTypesList.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading incentive types: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void HandleIncentiveTypeCheckBoxChanged(CheckBox checkBox, List<string> selectedTypesList)
        {
            if (checkBox == null) return;

            string incentiveTypeID = checkBox.Tag?.ToString();

            if (!string.IsNullOrEmpty(incentiveTypeID))
            {
                if (checkBox.IsChecked == true)
                {
                    if (!selectedTypesList.Contains(incentiveTypeID))
                    {
                        selectedTypesList.Add(incentiveTypeID);
                    }
                }
                else
                {
                    selectedTypesList.Remove(incentiveTypeID);
                }
            }
        }

        private void ConfigureTabsForAccess(bool includeAdmin, TabItem defaultSelectedTab)
        {
            // Show common tabs for authenticated users
            tabBusinessSearch.Visibility = Visibility.Visible;
            tabBusinessUpdate.Visibility = Visibility.Visible;
            tabBusinessAdd.Visibility = Visibility.Visible;
            tabIncentiveView.Visibility = Visibility.Visible;
            tabIncentiveUpdate.Visibility = Visibility.Visible;
            tabIncentiveAdd.Visibility = Visibility.Visible;

            // Show admin tab if user has admin access
            if (includeAdmin)
            {
                tabAdmin.Visibility = Visibility.Visible;
            }

            // Set selection state for all tabs
            tabBusinessSearch.IsSelected = (defaultSelectedTab == tabBusinessSearch);
            tabBusinessUpdate.IsSelected = (defaultSelectedTab == tabBusinessUpdate);
            tabBusinessAdd.IsSelected = (defaultSelectedTab == tabBusinessAdd);
            tabIncentiveView.IsSelected = (defaultSelectedTab == tabIncentiveView);
            tabIncentiveUpdate.IsSelected = (defaultSelectedTab == tabIncentiveUpdate);
            tabIncentiveAdd.IsSelected = (defaultSelectedTab == tabIncentiveAdd);
            tabAdmin.IsSelected = (defaultSelectedTab == tabAdmin);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (btnLogin.Content == "Log Out")
            {
                logout();
                return;
            }

            string username = txtEmail.Text.Trim();
            string password = pwdPassword.Password.Trim();

            if (username == "" || username == null)
            {
                MessageBox.Show("Email cannot be empty.");
                txtEmail.Focus();
                return;
            }

            if (password == "" || password == null)
            {
                MessageBox.Show("Password cannot be empty.");
                pwdPassword.Focus();
                return;
            }

            try
            {
                _accessToken = _userManager.loginUser(username, password);

                if (_accessToken != null)
                {
                    // DEBUG: Check what value we're getting
                    System.Diagnostics.Debug.WriteLine($"Login successful for: {_accessToken.Email}");
                    System.Diagnostics.Debug.WriteLine($"MustUpdatePassword value: {_accessToken.MustUpdatePassword}");

                    // Set current user for per-user preferences
                    UserPreferences.SetCurrentUser(_accessToken.UserID);

                    // Apply user's saved preferences (theme, font size, etc.)
                    PreferencesHelper.ApplyTheme(this);
                    PreferencesHelper.ApplyFontSize(this);

                    // CHECK IF PASSWORD UPDATE IS REQUIRED
                    if (_accessToken.MustUpdatePassword)
                    {
                        if (!updatePassword(_accessToken.MustUpdatePassword).GetValueOrDefault())
                        {
                            // User cancelled or password update failed - log them out
                            logout();
                            MessageBox.Show("You must update your password to continue" + "\nLogging You Out!");
                            return;
                        }
                        // If we get here, password was updated successfully and user stays logged in
                    }

                    // If we get this far, the user is logged in!
                    setUpUi();
                }
                else
                {
                    MessageBox.Show("Invalid email or password. Please try again.",
                        "Login Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    txtEmail.Clear();
                    pwdPassword.Clear();
                    txtEmail.Focus();
                }
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message,
                    "Login Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtEmail.Clear();
                pwdPassword.Clear();
                txtEmail.Focus();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nDetails: " + ex.InnerException.Message;
                }

                MessageBox.Show(errorMessage,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtEmail.Clear();
                pwdPassword.Clear();
                txtEmail.Focus();
            }
        }

        private void setUpUi()
        {
            btnLogin.IsDefault = false;
            lblGreeting.Content = "Welcome Back, " + _accessToken.FirstName + ".";
            txtEmail.Clear();
            pwdPassword.Clear();
            txtEmail.Visibility = Visibility.Hidden;
            lblEmail.Visibility = Visibility.Hidden;
            pwdPassword.Visibility = Visibility.Hidden;
            lblPassword.Visibility = Visibility.Hidden;
            btnLogin.Content = "Log Out";

            string memLevel = "";

            if (_accessToken.Access != null && _accessToken.Access.Count > 0)
            {
                memLevel = _accessToken.Access[0];  // Get the first (and should be only) membership level
                statMessage.Content = "Your membership level is: " + memLevel + ".";
            }
            else
            {
                statMessage.Content = "No membership level found.";
            }

            // one true brace style (OTB style)
            foreach (var access in _accessToken.Access)
            {
                switch (access)
                {
                    case "Guest":
                        tabBusinessSearch.Visibility = Visibility.Visible;
                        tabBusinessSearch.IsSelected = true;
                        break;
                    case "Member":
                    case "TestAccess":
                        ConfigureTabsForAccess(includeAdmin: false, defaultSelectedTab: tabBusinessSearch);
                        break;
                    //case "Premium":  // Future implementation
                    //    break;
                    //case "VIP": // Future implementation
                    //    break;
                    case "Admin":
                        ConfigureTabsForAccess(includeAdmin: true, defaultSelectedTab: tabAdmin);
                        break;

                }
                tabContainer.Visibility = Visibility.Visible;
            }

        }

        private void logout()
        {
            _accessToken = null;

            // Clear current user for preferences
            UserPreferences.ClearCurrentUser();

            // Revert to guest/default preferences
            PreferencesHelper.ApplyTheme(this);
            PreferencesHelper.ApplyFontSize(this);

            txtEmail.Visibility = Visibility.Visible;
            lblEmail.Visibility = Visibility.Visible;
            pwdPassword.Visibility = Visibility.Visible;
            lblPassword.Visibility = Visibility.Visible;
            btnLogin.Content = "Log In";
            lblGreeting.Content = "You are not logged in.";
            btnLogin.IsDefault = true;
            tabContainer.Visibility = Visibility.Hidden;
            _adminTabInitialized = false;
            foreach (var tabItem in tabSetMain.Items)
            {
                ((TabItem)tabItem).Visibility = Visibility.Collapsed;
            }
            statMessage.Content = "Welcome, Please log in to continue.";
        }

        private void mnuChangePassword_Click(object sender, RoutedEventArgs e)
        {
            updatePassword();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit Patriot Thanks?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void mnuPreferences_Click(object sender, RoutedEventArgs e)
        {
            var preferencesWindow = new PreferencesWindow();
            preferencesWindow.Owner = this;
            if (preferencesWindow.ShowDialog() == true)
            {
                // Preferences were saved - reload and apply them
                PreferencesHelper.ApplyAllPreferences(
                    this,
                    cmbSearchState,
                    txtSearchBusinessName,
                    txtSearchCity,
                    txtSearchZip,
                    tabSetMain
                );
            }
        }

        private void mnuPrivacyPolicy_Click(object sender, RoutedEventArgs e)
        {
            var privacyWindow = new PrivacyPolicyWindow();
            privacyWindow.Owner = this;
            privacyWindow.ShowDialog();
        }

        private void mnuTermsAndConditions_Click(object sender, RoutedEventArgs e)
        {
            var termsWindow = new TermsOfUseWindow();
            termsWindow.Owner = this;
            termsWindow.ShowDialog();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void mnuContactUs_Click(object sender, RoutedEventArgs e)
        {
            var contactWindow = new ContactUsWindow();
            contactWindow.Owner = this;
            contactWindow.ShowDialog();
        }

        private void mnuDonation_Click(object sender, RoutedEventArgs e)
        {
            var donationWindow = new DonationWindow();
            donationWindow.Owner = this;
            donationWindow.ShowDialog();
        }

        private void ApplyUserPreferences()
        {
            // Use the helper class to apply all preferences
            PreferencesHelper.ApplyAllPreferences(
                this,
                cmbSearchState,           // Your state ComboBox
                txtSearchBusinessName,    // Your business name TextBox
                txtSearchCity,            // Your city TextBox  
                txtSearchZip,             // Your zip TextBox
                tabSetMain                // Your TabControl
            );
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window position
            PreferencesHelper.SaveWindowPosition(this);

            // Save last tab
            PreferencesHelper.SaveLastTab(tabSetMain);
        }

        private bool? updatePassword(bool forced=false)
        {
            bool? isUpdated = false;

            if (_accessToken != null)
            {
                UpdatePasswordWindow updatePassword = new UpdatePasswordWindow(_userManager, _accessToken, forced);

                isUpdated = updatePassword.ShowDialog();
            }
            else
            {
                MessageBox.Show("You must be Logged in for this action.");
            }
            return isUpdated;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logout();

            PreferencesHelper.ApplyAllPreferences(
                this,
                cmbSearchState,
                txtSearchBusinessName,
                txtSearchCity,
                txtSearchZip,
                tabSetMain
            );
        }

        private void RefreshUserGrid()
        {
            try
            {
                var users = _userManager.GetUsersByActive("Active");
                datUsers.ItemsSource = users;
                statMessage.Content = $"Displaying {users.Count} active users.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void tabSetMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != tabSetMain)
                return;

            try
            {
                if (tabSetMain.SelectedItem == tabBusinessSearch)
                {
                    statMessage.Content = "Enter search criteria and press Enter or click Search";

                    if (cmbSearchBusinessType != null && cmbSearchBusinessType.Items.Count == 0)
                    {
                        LoadBusinessTypes(cmbSearchBusinessType, true);  // "All Types"
                    }

                    if (cmbSearchState != null && cmbSearchState.Items.Count == 0)
                    {
                        LoadStates(cmbSearchState, true);  // "All States"
                        PreferencesHelper.ApplyDefaultState(cmbSearchState);
                    }
                }
                // Business Add Tab
                else if (tabSetMain.SelectedItem == tabBusinessAdd)
                {
                    statMessage.Content = "Fill out the form to add a new business";

                    if (cmbAddBusinessType != null && cmbAddBusinessType.Items.Count == 0)
                    {
                        LoadBusinessTypes(cmbAddBusinessType, false);  // "-- Select --"
                    }

                    if (cmbAddState != null && cmbAddState.Items.Count == 0)
                    {
                        LoadStates(cmbAddState, false);  // "-- Select --"
                        PreferencesHelper.ApplyDefaultState(cmbAddState);
                    }
                }
                // Business Update Tab
                else if (tabSetMain.SelectedItem == tabBusinessUpdate)
                {
                    statMessage.Content = "Search for a business to update";

                    if (cmbUpdateSearchBusinessType != null && cmbUpdateSearchBusinessType.Items.Count == 0)
                    {
                        LoadBusinessTypes(cmbUpdateSearchBusinessType, true);
                    }

                    if (cmbUpdateSearchState != null && cmbUpdateSearchState.Items.Count == 0)
                    {
                        LoadStates(cmbUpdateSearchState, true);
                        PreferencesHelper.ApplyDefaultState(cmbUpdateSearchState);
                    }

                    // Also load dropdowns for the edit form
                    if (cmbUpdateBusinessType != null && cmbUpdateBusinessType.Items.Count == 0)
                    {
                        LoadBusinessTypes(cmbUpdateBusinessType, false);
                    }

                    if (cmbUpdateState != null && cmbUpdateState.Items.Count == 0)
                    {
                        LoadStates(cmbUpdateState, false);
                        PreferencesHelper.ApplyDefaultState(cmbUpdateState);
                    }
                }
                // Incentive View Tab
                else if (tabSetMain.SelectedItem == tabIncentiveView)
                {

                    if (cmbIncentiveSearchBusinessType != null && cmbIncentiveSearchBusinessType.Items.Count == 0)
                    {
                        LoadBusinessTypes(cmbIncentiveSearchBusinessType, true);
                    }

                    if (cmbIncentiveSearchState != null && cmbIncentiveSearchState.Items.Count == 0)
                    {
                        LoadStates(cmbIncentiveSearchState, true);
                        PreferencesHelper.ApplyDefaultState(cmbIncentiveSearchState);
                    }

                    // Also load dropdowns for the edit form
                    if (cmbIncentiveSearchBusinessType != null && cmbIncentiveSearchBusinessType.Items.Count == 0)
                    {
                        LoadBusinessTypes(cmbIncentiveSearchBusinessType, false);
                    }

                    if (cmbIncentiveSearchBusinessType != null && cmbIncentiveSearchBusinessType.Items.Count == 0)
                    {
                        LoadStates(cmbIncentiveSearchState, false);
                        PreferencesHelper.ApplyDefaultState(cmbIncentiveSearchState);
                    }
                }
                // Incentive Add Tab
                else if (tabSetMain.SelectedItem == tabIncentiveAdd)
                {
                    statMessage.Content = "Search for a business, then enter incentive details";

                    // Load incentive types if not already loaded
                    if (lstAddIncentiveTypes != null && lstAddIncentiveTypes.Items.Count == 0)
                    {
                        LoadIncentiveTypes();
                    }

                    // Set default start date to today
                    if (dpAddIncentiveStartDate != null && !dpAddIncentiveStartDate.SelectedDate.HasValue)
                    {
                        dpAddIncentiveStartDate.SelectedDate = DateTime.Today;
                    }
                }
                // Incentive Update Tab
                else if (tabSetMain.SelectedItem == tabIncentiveUpdate)
                {
                    statMessage.Content = "Search for an incentive to update";

                    // Load business types if not already loaded
                    if (cmbUpdateIncentiveSearchBusinessType != null && cmbUpdateIncentiveSearchBusinessType.Items.Count == 0)
                    {
                        LoadBusinessTypes(cmbUpdateIncentiveSearchBusinessType, true);
                    }

                    // Load states if not already loaded
                    if (cmbUpdateIncentiveSearchState != null && cmbUpdateIncentiveSearchState.Items.Count == 0)
                    {
                        LoadStates(cmbUpdateIncentiveSearchState, true);
                        PreferencesHelper.ApplyDefaultState(cmbUpdateIncentiveSearchState);
                    }

                    // Load incentive types for the edit form
                    if (lstUpdateIncentiveTypes != null && lstUpdateIncentiveTypes.Items.Count == 0)
                    {
                        LoadUpdateIncentiveTypes();
                    }
                }
                // Admin Tab
                else if (tabSetMain.SelectedItem == tabAdmin)
                {
                    System.Diagnostics.Debug.WriteLine("Admin tab selected");

                    if (_accessToken != null && _accessToken.Access != null
                        && _accessToken.Access.Contains("Admin"))
                    {
                        if (!_adminTabInitialized)
                        {
                            System.Diagnostics.Debug.WriteLine("Initializing Admin tab");

                            if (cmbUserAccountStatusFilter != null && cmbUserAccountStatusFilter.Items.Count == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Loading account status filter");
                                LoadAccountStatusFilter();
                            }
                            RefreshUserGrid();
                            _adminTabInitialized = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing tabs: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadBusinessTypes(ComboBox comboBox, bool includeAllOption = true)
        {
            try
            {
                if (comboBox == null)
                {
                    return;
                }

                comboBox.Items.Clear();

                // Add first option based on context
                if (includeAllOption)
                {
                    comboBox.Items.Add(new ComboBoxItem
                    {
                        Content = "All Types",
                        Tag = ""
                    });
                }
                else
                {
                    comboBox.Items.Add(new ComboBoxItem
                    {
                        Content = "-- Select Business Type --",
                        Tag = ""
                    });
                }

                var businessTypes = _lookupManager.GetActiveBusinessTypes();

                foreach (var busType in businessTypes)
                {
                    comboBox.Items.Add(new ComboBoxItem
                    {
                        Content = busType.BusTypeID,
                        Tag = busType.BusTypeID
                    });
                }

                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading business types: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadStates(ComboBox comboBox, bool includeAllOption = true)
        {
            try
            {
                if (comboBox == null)
                {
                    return;
                }

                comboBox.Items.Clear();

                if (includeAllOption)
                {
                    comboBox.Items.Add(new ComboBoxItem
                    {
                        Content = "All States",
                        Tag = ""
                    });
                }
                else
                {
                    comboBox.Items.Add(new ComboBoxItem
                    {
                        Content = "-- Select State --",
                        Tag = ""
                    });
                }

                var states = _lookupManager.GetAllStates();

                foreach (var state in states)
                {
                    comboBox.Items.Add(new ComboBoxItem
                    {
                        Content = $"{state.StateID} - {state.StateDescription}",
                        Tag = state.StateID
                    });
                }

                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading states: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(sender, e);
                btnIncentiveSearch_Click(sender, e);
                e.Handled = true;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statMessage.Content = "Searching...";

                var criteria = new BusinessSearchCriteria
                {
                    BusinessName = txtSearchBusinessName.Text.Trim(),
                    StreetAddress = txtSearchStreetAddress.Text.Trim(),
                    City = txtSearchCity.Text.Trim(),
                    StateID = ((ComboBoxItem)cmbSearchState.SelectedItem)?.Tag?.ToString(),
                    Zip = txtSearchZip.Text.Trim(),
                    BusinessTypeID = ((ComboBoxItem)cmbSearchBusinessType.SelectedItem)?.Tag?.ToString(),
                    IsActive = true
                };

                var results = _businessManager.SearchBusinesses(criteria);
                datBusinessResults.ItemsSource = results;

                if (results.Count > 0)
                {
                    statMessage.Content = $"Found {results.Count} business(es)";
                }
                else
                {
                    statMessage.Content = "No businesses found. Try different search criteria.";
                }
            }
            catch (Exception ex)
            {
                statMessage.Content = "Search failed. Please try again.";
                MessageBox.Show($"Error searching businesses: {ex.Message}",
                    "Search Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            PreferencesHelper.SaveLastSearch(
                txtSearchBusinessName,
                txtSearchCity,
                cmbSearchState,
                txtSearchZip
            );
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearchBusinessName.Clear();
            txtSearchStreetAddress.Clear();
            txtSearchCity.Clear();
            txtSearchZip.Clear();
            cmbSearchState.SelectedIndex = 0;
            cmbSearchBusinessType.SelectedIndex = 0;
            datBusinessResults.ItemsSource = null;
            statMessage.Content = "Search cleared. Enter new criteria and press Enter or click Search.";
        }

        private void datBusinessResults_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (datBusinessResults.SelectedItem == null)
            {
                return;
            }

            try
            {
                var selectedBusiness = (Business)datBusinessResults.SelectedItem;

                // Get fresh business data with full details
                var business = _businessManager.GetBusinessByID(selectedBusiness.BusinessID);

                if (business == null)
                {
                    MessageBox.Show("Could not retrieve business details.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Open the Business Details dialog
                var detailsWindow = new BusinessDetailsWindow(business, _incentiveManager);
                detailsWindow.Owner = this;

                bool? dialogResult = detailsWindow.ShowDialog();

                if (dialogResult == true)
                {
                    // Use Dispatcher to ensure UI updates happen after dialog fully closes
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Handle the user's selection
                        if (detailsWindow.EditBusinessRequested)
                        {
                            // Navigate to Update Business tab and load this business
                            tabSetMain.SelectedItem = tabBusinessUpdate;
                            LoadBusinessForEditing(business.BusinessID);
                        }
                        else if (detailsWindow.ViewIncentivesRequested)
                        {
                            // Navigate to View Incentives tab and search for this business
                            tabSetMain.SelectedItem = tabIncentiveView;
                            NavigateToIncentiveViewForBusiness(business);
                        }
                        else if (detailsWindow.AddIncentiveRequested)
                        {
                            // Navigate to Add Incentive tab and pre-select this business
                            tabSetMain.SelectedItem = tabIncentiveAdd;
                            NavigateToAddIncentiveForBusiness(business);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading business details: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NavigateToIncentiveViewForBusiness(Business business)
        {
            try
            {
                // Clear any existing search criteria and set business name
                txtIncentiveSearchBusinessName.Text = business.BusinessName;
                txtIncentiveSearchCity.Text = "";
                cmbIncentiveSearchState.SelectedIndex = 0;
                chkIncludeFutureIncentives.IsChecked = false;

                // Perform the search
                var searchCriteria = new IncentiveSearchCriteria
                {
                    BusinessID = business.BusinessID
                };

                var results = _incentiveManager.SearchIncentives(searchCriteria);
                datBusinessIncentiveResults.ItemsSource = results;

                statMessage.Content = $"Found {results.Count} incentive(s) for {business.BusinessName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching incentives: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NavigateToAddIncentiveForBusiness(Business business)
        {
            try
            {
                // Set the business as the selected business for incentive
                _selectedBusinessForIncentive = business;

                // Update the UI to show the selected business
                txtAddIncentiveBusinessSearch.Text = business.BusinessName;
                txtAddIncentiveCitySearch.Text = "";

                // Show the business in the search results
                datAddIncentiveBusinessResults.ItemsSource = new List<Business> { business };

                // Show the incentive form
                grpAddIncentiveForm.Visibility = Visibility.Visible;

                // Clear the form fields for a new incentive
                txtAddIncentiveAmount.Text = "";
                chkAddIncentiveIsPercentage.IsChecked = true;
                txtAddIncentiveDescription.Text = "";
                txtAddIncentiveLimitations.Text = "";
                dpAddIncentiveStartDate.SelectedDate = DateTime.Today;
                dpAddIncentiveEndDate.SelectedDate = null;

                // Clear incentive type selections
                _selectedIncentiveTypeIDs.Clear();
                if (lstAddIncentiveTypes != null)
                {
                    lstAddIncentiveTypes.SelectedItems.Clear();
                }

                statMessage.Content = $"Adding incentive for: {business.BusinessName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing incentive form: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnAddBusiness_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtAddBusinessName.Text))
            {
                MessageBox.Show("Business Name is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtAddBusinessName.Focus();
                return;
            }

            if (cmbAddBusinessType.SelectedItem == null ||
                string.IsNullOrWhiteSpace(((ComboBoxItem)cmbAddBusinessType.SelectedItem).Tag?.ToString()))
            {
                MessageBox.Show("Business Type is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                cmbAddBusinessType.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAddStreetAddress.Text))
            {
                MessageBox.Show("Street Address is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtAddStreetAddress.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAddCity.Text))
            {
                MessageBox.Show("City is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtAddCity.Focus();
                return;
            }

            if (cmbAddState.SelectedItem == null ||
                string.IsNullOrWhiteSpace(((ComboBoxItem)cmbAddState.SelectedItem).Tag?.ToString()))
            {
                MessageBox.Show("State is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                cmbAddState.Focus();
                return;
            }

            try
            {
                // Create business object with all fields
                var newBusiness = new Business
                {
                    BusinessName = txtAddBusinessName.Text.Trim(),
                    BusinessTypeID = ((ComboBoxItem)cmbAddBusinessType.SelectedItem).Tag.ToString(),
                    Phone = txtAddPhone.Text.Trim(),
                    LocationName = txtAddLocationName.Text.Trim(),
                    StreetAddress = txtAddStreetAddress.Text.Trim(),
                    Address2 = txtAddAddress2.Text.Trim(),
                    City = txtAddCity.Text.Trim(),
                    StateID = ((ComboBoxItem)cmbAddState.SelectedItem).Tag.ToString(),
                    Zip = txtAddZip.Text.Trim()
                };

                // Add to database
                int newBusinessID = _businessManager.AddBusiness(newBusiness);

                if (newBusinessID > 0)
                {
                    string addedBusinessName = newBusiness.BusinessName;

                    MessageBox.Show($"Business added successfully!\nBusiness ID: {newBusinessID}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Clear form
                    btnClearAddBusiness_Click(sender, e);

                    // Refresh the results grid to show the new business
                    var criteria = new BusinessSearchCriteria
                    {
                        BusinessName = addedBusinessName,
                        IsActive = true
                    };
                    var results = _businessManager.SearchBusinesses(criteria);
                    datBusinessAddResults.ItemsSource = results;

                    statMessage.Content = $"Business '{newBusiness.BusinessName}' added with ID {newBusinessID}";
                }
                else
                {
                    MessageBox.Show("Failed to add business. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nDetails: " + ex.InnerException.Message;
                }

                MessageBox.Show($"Error adding business:\n\n{errorMessage}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnClearAddBusiness_Click(object sender, RoutedEventArgs e)
        {
            txtAddBusinessName.Clear();
            txtAddPhone.Clear();
            txtAddLocationName.Clear();
            txtAddStreetAddress.Clear();
            txtAddAddress2.Clear();
            txtAddCity.Clear();
            txtAddZip.Clear();

            if (cmbAddBusinessType.Items.Count > 0)
            {
                cmbAddBusinessType.SelectedIndex = 0;
            }

            if (cmbAddState.Items.Count > 0)
            {
                cmbAddState.SelectedIndex = 0;
            }

            datBusinessAddResults.ItemsSource = null;
            statMessage.Content = "Form cleared. Enter business details to add.";
            txtAddBusinessName.Focus();
        }

        private void datBusinessAddResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datBusinessAddResults.SelectedItem != null)
            {
                var selectedBusiness = (Business)datBusinessAddResults.SelectedItem;
                MessageBox.Show($"Selected: {selectedBusiness.BusinessName}\n{selectedBusiness.FullAddress}",
                    "Business Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void btnUpdateSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statMessage.Content = "Searching...";

                var criteria = new BusinessSearchCriteria
                {
                    BusinessName = txtUpdateSearchBusinessName.Text.Trim(),
                    City = txtUpdateSearchCity.Text.Trim(),
                    StateID = ((ComboBoxItem)cmbUpdateSearchState.SelectedItem)?.Tag?.ToString(),
                    BusinessTypeID = ((ComboBoxItem)cmbUpdateSearchBusinessType.SelectedItem)?.Tag?.ToString(),
                    IsActive = true
                };

                var results = _businessManager.SearchBusinesses(criteria);
                datUpdateBusinessResults.ItemsSource = results;

                if (results.Count > 0)
                {
                    statMessage.Content = $"Found {results.Count} business(es). Double-click to edit.";
                }
                else
                {
                    statMessage.Content = "No businesses found. Try different search criteria.";
                }

                // Hide edit form, show results
                grpUpdateBusinessForm.Visibility = Visibility.Collapsed;
                grpUpdateBusinessResults.Visibility = Visibility.Visible;
                _selectedBusinessForUpdate = null;
            }
            catch (Exception ex)
            {
                statMessage.Content = "Search failed. Please try again.";
                MessageBox.Show($"Error searching businesses: {ex.Message}",
                    "Search Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnUpdateClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtUpdateSearchBusinessName.Clear();
            txtUpdateSearchCity.Clear();
            cmbUpdateSearchState.SelectedIndex = 0;
            cmbUpdateSearchBusinessType.SelectedIndex = 0;
            datUpdateBusinessResults.ItemsSource = null;
            grpUpdateBusinessForm.Visibility = Visibility.Collapsed;
            grpUpdateBusinessResults.Visibility = Visibility.Visible;
            _selectedBusinessForUpdate = null;
            statMessage.Content = "Search cleared. Enter criteria and click Search.";
        }

        private void datUpdateBusinessResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datUpdateBusinessResults.SelectedItem != null)
            {
                var selectedBusiness = (Business)datUpdateBusinessResults.SelectedItem;
                LoadBusinessForEditing(selectedBusiness.BusinessID);
            }
        }

        private void LoadBusinessForEditing(int businessID)
        {
            try
            {
                // Get fresh data from database
                _selectedBusinessForUpdate = _businessManager.GetBusinessByID(businessID);

                if (_selectedBusinessForUpdate == null)
                {
                    MessageBox.Show("Business not found.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Populate form fields
                txtUpdateBusinessName.Text = _selectedBusinessForUpdate.BusinessName;
                txtUpdatePhone.Text = _selectedBusinessForUpdate.Phone;
                txtUpdateStreetAddress.Text = _selectedBusinessForUpdate.StreetAddress;
                txtUpdateAddress2.Text = _selectedBusinessForUpdate.Address2;
                txtUpdateCity.Text = _selectedBusinessForUpdate.City;
                txtUpdateZip.Text = _selectedBusinessForUpdate.Zip;
                chkUpdateIsActive.IsChecked = _selectedBusinessForUpdate.IsActive;

                // Set ComboBox selections
                SetComboBoxByTag(cmbUpdateBusinessType, _selectedBusinessForUpdate.BusinessTypeID);
                SetComboBoxByTag(cmbUpdateState, _selectedBusinessForUpdate.StateID);

                // Show edit form
                // Show edit form, hide results
                grpUpdateBusinessForm.Visibility = Visibility.Visible;
                grpUpdateBusinessResults.Visibility = Visibility.Collapsed;
                statMessage.Content = $"Editing: {_selectedBusinessForUpdate.BusinessName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading business: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnUpdateSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBusinessForUpdate == null)
            {
                MessageBox.Show("No business selected for update.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(txtUpdateBusinessName.Text))
            {
                ShowValidationError("Business Name is required.", txtUpdateBusinessName);
                return;
            }

            if (cmbUpdateBusinessType.SelectedItem == null ||
                string.IsNullOrWhiteSpace(((ComboBoxItem)cmbUpdateBusinessType.SelectedItem).Tag?.ToString()))
            {
                ShowValidationError("Business Type is required.", cmbUpdateBusinessType);
                return;
            }

            if (cmbUpdateState.SelectedItem == null ||
                string.IsNullOrWhiteSpace(((ComboBoxItem)cmbUpdateState.SelectedItem).Tag?.ToString()))
            {
                ShowValidationError("State is required.", cmbUpdateState);
                return;
            }

            try
            {
                // Update business object with form values
                _selectedBusinessForUpdate.BusinessName = txtUpdateBusinessName.Text.Trim();
                _selectedBusinessForUpdate.BusinessTypeID = ((ComboBoxItem)cmbUpdateBusinessType.SelectedItem).Tag.ToString();
                _selectedBusinessForUpdate.Phone = txtUpdatePhone.Text.Trim();
                _selectedBusinessForUpdate.StreetAddress = txtUpdateStreetAddress.Text.Trim();
                _selectedBusinessForUpdate.Address2 = txtUpdateAddress2.Text.Trim();
                _selectedBusinessForUpdate.City = txtUpdateCity.Text.Trim();
                _selectedBusinessForUpdate.StateID = ((ComboBoxItem)cmbUpdateState.SelectedItem).Tag.ToString();
                _selectedBusinessForUpdate.Zip = txtUpdateZip.Text.Trim();
                _selectedBusinessForUpdate.IsActive = chkUpdateIsActive.IsChecked ?? true;

                // Save to database
                bool success = _businessManager.UpdateBusiness(_selectedBusinessForUpdate);

                if (success)
                {
                    MessageBox.Show("Business updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Refresh search results
                    btnUpdateSearch_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("Failed to update business. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating business: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnUpdateCancel_Click(object sender, RoutedEventArgs e)
        {
            grpUpdateBusinessForm.Visibility = Visibility.Collapsed;
            grpUpdateBusinessResults.Visibility = Visibility.Visible;
            _selectedBusinessForUpdate = null;
            statMessage.Content = "Update cancelled. Select a business to edit.";
        }

        private void SetComboBoxByTag(ComboBox comboBox, string tagValue)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Tag?.ToString() == tagValue)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
            comboBox.SelectedIndex = 0; // Default to first item if not found
        }

        private void btnIncentiveSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statMessage.Content = "Searching...";

                var criteria = new IncentiveSearchCriteria
                {
                    // Business search fields from UI
                    BusinessName = txtIncentiveSearchBusinessName.Text.Trim(),
                    StreetAddress = txtIncentiveSearchStreetAddress.Text.Trim(),
                    City = txtIncentiveSearchCity.Text.Trim(),
                    StateID = ((ComboBoxItem)cmbIncentiveSearchState.SelectedItem)?.Tag?.ToString(),
                    Zip = txtIncentiveSearchZip.Text.Trim(),
                    BusinessTypeID = ((ComboBoxItem)cmbIncentiveSearchBusinessType.SelectedItem)?.Tag?.ToString(),
                    BusinessID = null,
                    IncentiveTypeID = null,
                    MinAmount = null,
                    MaxAmount = null,
                    ActiveOnly = !(chkIncludeFutureIncentives.IsChecked ?? false),
                };

                var results = _incentiveManager.SearchIncentives(criteria);
                datBusinessIncentiveResults.ItemsSource = results;

                if (results.Count > 0)
                {
                    statMessage.Content = $"Found {results.Count} business(es) with Incentives";
                }
                else
                {
                    statMessage.Content = "No businesses found. Try different search criteria.";
                }
            }
            catch (Exception ex)
            {
                statMessage.Content = "Search failed. Please try again.";

                // Build complete error message including all inner exceptions
                string errorMessage = ex.Message;
                Exception inner = ex.InnerException;
                while (inner != null)
                {
                    errorMessage += "\n\n→ " + inner.Message;
                    inner = inner.InnerException;
                }

                MessageBox.Show($"Error searching Incentives:\n\n{errorMessage}",
                    "Search Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnIncentiveClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtIncentiveSearchBusinessName.Clear();
            txtIncentiveSearchStreetAddress.Clear();
            txtIncentiveSearchCity.Clear();
            txtIncentiveSearchZip.Clear();
            cmbIncentiveSearchState.SelectedIndex = 0;
            cmbIncentiveSearchBusinessType.SelectedIndex = 0;
            chkIncludeFutureIncentives.IsChecked = false;
            datBusinessIncentiveResults.ItemsSource = null;
            statMessage.Content = "Search cleared. Enter criteria and click Search.";
        }

        private void datBusinessIncentiveResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datBusinessIncentiveResults.SelectedItem == null)
            {
                return;
            }

            try
            {
                var selectedIncentive = (Incentive)datBusinessIncentiveResults.SelectedItem;

                // Get the full business details
                var business = _businessManager.GetBusinessByID(selectedIncentive.BusinessID);

                if (business == null)
                {
                    MessageBox.Show("Could not retrieve business details.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Build the details message
                StringBuilder details = new StringBuilder();

                // Business Information Section
                details.AppendLine("═══════════════════════════════════════");
                details.AppendLine("              BUSINESS INFORMATION");
                details.AppendLine("═══════════════════════════════════════");
                details.AppendLine();
                details.AppendLine($"Business Name:  {business.BusinessName}");
                details.AppendLine();
                details.AppendLine("Address:");
                details.AppendLine($"  {business.StreetAddress}");
                if (!string.IsNullOrWhiteSpace(business.Address2))
                {
                    details.AppendLine($"  {business.Address2}");
                }
                details.AppendLine($"  {business.City}, {business.StateID} {business.Zip}");
                if (!string.IsNullOrWhiteSpace(business.LocationName))
                {
                    details.AppendLine();
                    details.AppendLine($"Location:       {business.LocationName}");
                }
                details.AppendLine();
                details.AppendLine($"Phone:          {(string.IsNullOrWhiteSpace(business.Phone) ? "Not on file" : business.Phone)}");
                details.AppendLine();

                // Incentive Information Section
                details.AppendLine("═══════════════════════════════════════");
                details.AppendLine("              INCENTIVE DETAILS");
                details.AppendLine("═══════════════════════════════════════");
                details.AppendLine();
                details.AppendLine($"Amount:         {selectedIncentive.FormattedAmount}");

                // Display incentive types as a list
                if (!string.IsNullOrWhiteSpace(selectedIncentive.IncentiveTypesDisplay))
                {
                    details.AppendLine();
                    details.AppendLine("Type(s):");
                    var types = selectedIncentive.IncentiveTypesDisplay.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var type in types)
                    {
                        details.AppendLine($"  • {type.Trim()}");
                    }
                }

                details.AppendLine();
                details.AppendLine($"Description:");
                details.AppendLine($"  {selectedIncentive.IncentiveDescription}");

                if (!string.IsNullOrWhiteSpace(selectedIncentive.Limitations))
                {
                    details.AppendLine();
                    details.AppendLine($"Limitations:");
                    details.AppendLine($"  {selectedIncentive.Limitations}");
                }

                details.AppendLine();
                details.AppendLine($"Valid:          {selectedIncentive.DateRangeDisplay}");
                details.AppendLine($"Status:         {(selectedIncentive.IsCurrentlyActive ? "Currently Active" : "Not Active")}");

                // Show the details in a MessageBox
                MessageBox.Show(details.ToString(),
                    $"Incentive Details - {business.BusinessName}",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading details: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadIncentiveTypes()
        {
            LoadIncentiveTypesToListBox(lstAddIncentiveTypes, _selectedIncentiveTypeIDs);
        }

        private void btnAddIncentiveSearchBusiness_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statMessage.Content = "Searching for businesses...";

                var criteria = new BusinessSearchCriteria
                {
                    BusinessName = txtAddIncentiveBusinessSearch.Text.Trim(),
                    City = txtAddIncentiveCitySearch.Text.Trim(),
                    IsActive = true
                };

                var results = _businessManager.SearchBusinesses(criteria);
                datAddIncentiveBusinessResults.ItemsSource = results;

                if (results.Count > 0)
                {
                    statMessage.Content = $"Found {results.Count} business(es). Double-click to select.";
                }
                else
                {
                    statMessage.Content = "No businesses found. Try different search criteria.";
                }
            }
            catch (Exception ex)
            {
                statMessage.Content = "Search failed. Please try again.";
                MessageBox.Show($"Error searching businesses: {ex.Message}",
                    "Search Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void datAddIncentiveBusinessResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datAddIncentiveBusinessResults.SelectedItem != null)
            {
                _selectedBusinessForIncentive = (Business)datAddIncentiveBusinessResults.SelectedItem;
                txtSelectedBusinessForIncentive.Text = $"{_selectedBusinessForIncentive.BusinessName} - {_selectedBusinessForIncentive.City}, {_selectedBusinessForIncentive.StateID} (ID: {_selectedBusinessForIncentive.BusinessID})";
                txtSelectedBusinessForIncentive.FontStyle = FontStyles.Normal;
                txtSelectedBusinessForIncentive.Foreground = System.Windows.Media.Brushes.Black;
                // Show edit form
                grpAddIncentiveForm.Visibility = Visibility.Visible;
                statMessage.Content = $"Selected: {_selectedBusinessForIncentive.BusinessName}. Now enter incentive details.";
            }
        }

        private void IncentiveTypeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            HandleIncentiveTypeCheckBoxChanged(sender as CheckBox, _selectedIncentiveTypeIDs);
        }

        private void btnAddIncentiveSave_Click(object sender, RoutedEventArgs e)
        {
            // Validation - Business Selected
            if (_selectedBusinessForIncentive == null)
            {
                ShowValidationError("Please search and select a business first.", txtAddIncentiveBusinessSearch);
                return;
            }

            // Validation - Amount
            if (string.IsNullOrWhiteSpace(txtAddIncentiveAmount.Text))
            {
                ShowValidationError("Incentive Amount is required.", txtAddIncentiveAmount);
                return;
            }

            if (!decimal.TryParse(txtAddIncentiveAmount.Text.Trim(), out decimal amount) || amount < 0)
            {
                ShowValidationError("Please enter a valid positive number for Incentive Amount.", txtAddIncentiveAmount);
                txtAddIncentiveAmount.SelectAll();
                return;
            }

            // Validation - Description
            if (string.IsNullOrWhiteSpace(txtAddIncentiveDescription.Text))
            {
                ShowValidationError("Incentive Description is required.", txtAddIncentiveDescription);
                return;
            }

            // Validation - Start Date
            if (!dpAddIncentiveStartDate.SelectedDate.HasValue)
            {
                ShowValidationError("Start Date is required.", dpAddIncentiveStartDate);
                return;
            }

            // Validation - End Date (if provided, must be after Start Date)
            if (dpAddIncentiveEndDate.SelectedDate.HasValue &&
                dpAddIncentiveEndDate.SelectedDate.Value < dpAddIncentiveStartDate.SelectedDate.Value)
            {
                ShowValidationError("End Date must be after Start Date.", dpAddIncentiveEndDate);
                return;
            }

            // Validation - Incentive Types
            if (_selectedIncentiveTypeIDs.Count == 0)
            {
                ShowValidationError("Please select at least one Incentive Type.", lstAddIncentiveTypes);
                return;
            }

            try
            {
                // Create the incentive object
                var newIncentive = new Incentive
                {
                    BusinessID = _selectedBusinessForIncentive.BusinessID,
                    IncentiveAmount = amount,
                    IsPercentage = chkAddIncentiveIsPercentage.IsChecked ?? false,
                    IncentiveDescription = txtAddIncentiveDescription.Text.Trim(),
                    StartDate = dpAddIncentiveStartDate.SelectedDate.Value,
                    EndDate = dpAddIncentiveEndDate.SelectedDate,
                    Limitations = txtAddIncentiveLimitations.Text.Trim()
                };

                // Add to database
                int newIncentiveID = _incentiveManager.AddIncentive(newIncentive, _selectedIncentiveTypeIDs);

                if (newIncentiveID > 0)
                {

                    string businessName = _selectedBusinessForIncentive.BusinessName;

                    MessageBox.Show($"Incentive added successfully!\nIncentive ID: {newIncentiveID}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Clear the form
                    btnAddIncentiveClear_Click(sender, e);

                    statMessage.Content = $"Incentive ID {newIncentiveID} added for {businessName}";
                }
                else
                {
                    MessageBox.Show("Failed to add incentive. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nDetails: " + ex.InnerException.Message;
                }

                MessageBox.Show($"Error adding incentive:\n\n{errorMessage}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnAddIncentiveClear_Click(object sender, RoutedEventArgs e)
        {
            // Clear business selection
            _selectedBusinessForIncentive = null;
            txtSelectedBusinessForIncentive.Text = "No business selected. Search and double-click to select.";
            txtSelectedBusinessForIncentive.FontStyle = FontStyles.Italic;
            txtSelectedBusinessForIncentive.Foreground = System.Windows.Media.Brushes.Gray;

            // Clear search fields
            txtAddIncentiveBusinessSearch.Clear();
            txtAddIncentiveCitySearch.Clear();
            datAddIncentiveBusinessResults.ItemsSource = null;

            // Clear incentive fields
            txtAddIncentiveAmount.Clear();
            chkAddIncentiveIsPercentage.IsChecked = false;
            txtAddIncentiveDescription.Clear();
            dpAddIncentiveStartDate.SelectedDate = DateTime.Today;
            dpAddIncentiveEndDate.SelectedDate = null;
            txtAddIncentiveLimitations.Clear();

            // Clear incentive type selections
            _selectedIncentiveTypeIDs.Clear();

            // Uncheck all checkboxes in the ListBox
            foreach (var item in lstAddIncentiveTypes.Items)
            {
                var container = lstAddIncentiveTypes.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container != null)
                {
                    var checkBox = FindVisualChild<CheckBox>(container);
                    if (checkBox != null)
                    {
                        checkBox.IsChecked = false;
                    }
                }
            }

            statMessage.Content = "Form cleared. Search for a business to add an incentive.";
            txtAddIncentiveBusinessSearch.Focus();
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void btnUpdateIncentiveSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statMessage.Content = "Searching...";

                var criteria = new IncentiveSearchCriteria
                {
                    BusinessName = txtUpdateIncentiveSearchBusinessName.Text.Trim(),
                    City = txtUpdateIncentiveSearchCity.Text.Trim(),
                    StateID = ((ComboBoxItem)cmbUpdateIncentiveSearchState.SelectedItem)?.Tag?.ToString(),
                    BusinessTypeID = ((ComboBoxItem)cmbUpdateIncentiveSearchBusinessType.SelectedItem)?.Tag?.ToString(),
                    ActiveOnly = !(chkUpdateIncentiveIncludeFutureExpired.IsChecked ?? false)
                };

                var results = _incentiveManager.SearchIncentives(criteria);
                datUpdateIncentiveResults.ItemsSource = results;

                if (results.Count > 0)
                {
                    statMessage.Content = $"Found {results.Count} incentive(s). Double-click to edit.";
                }
                else
                {
                    statMessage.Content = "No incentives found. Try different search criteria.";
                }

                // Hide edit form when doing a new search
                grpUpdateIncentiveForm.Visibility = Visibility.Collapsed;
                _selectedIncentiveForUpdate = null;
            }
            catch (Exception ex)
            {
                statMessage.Content = "Search failed. Please try again.";

                string errorMessage = ex.Message;
                Exception inner = ex.InnerException;
                while (inner != null)
                {
                    errorMessage += "\n\n→ " + inner.Message;
                    inner = inner.InnerException;
                }

                MessageBox.Show($"Error searching incentives:\n\n{errorMessage}",
                    "Search Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnUpdateIncentiveClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtUpdateIncentiveSearchBusinessName.Clear();
            txtUpdateIncentiveSearchCity.Clear();
            cmbUpdateIncentiveSearchState.SelectedIndex = 0;
            cmbUpdateIncentiveSearchBusinessType.SelectedIndex = 0;
            chkUpdateIncentiveIncludeFutureExpired.IsChecked = false;
            datUpdateIncentiveResults.ItemsSource = null;
            grpUpdateIncentiveForm.Visibility = Visibility.Collapsed;
            _selectedIncentiveForUpdate = null;
            statMessage.Content = "Search cleared. Enter criteria and click Search.";
        }

        private void datUpdateIncentiveResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datUpdateIncentiveResults.SelectedItem != null)
            {
                var selectedIncentive = (Incentive)datUpdateIncentiveResults.SelectedItem;
                LoadIncentiveForEditing(selectedIncentive.IncentiveID);
            }
        }

        private void LoadIncentiveForEditing(int incentiveID)
        {
            try
            {
                // Get fresh data from database
                _selectedIncentiveForUpdate = _incentiveManager.GetIncentiveByID(incentiveID);

                if (_selectedIncentiveForUpdate == null)
                {
                    MessageBox.Show("Incentive not found.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Populate form fields
                txtUpdateIncentiveSelectedBusiness.Text = $"{_selectedIncentiveForUpdate.BusinessName} (ID: {_selectedIncentiveForUpdate.BusinessID})";
                txtUpdateIncentiveAmount.Text = _selectedIncentiveForUpdate.IncentiveAmount.ToString("0.##");
                chkUpdateIncentiveIsPercentage.IsChecked = _selectedIncentiveForUpdate.IsPercentage;
                txtUpdateIncentiveDescription.Text = _selectedIncentiveForUpdate.IncentiveDescription;
                dpUpdateIncentiveStartDate.SelectedDate = _selectedIncentiveForUpdate.StartDate;
                dpUpdateIncentiveEndDate.SelectedDate = _selectedIncentiveForUpdate.EndDate;
                txtUpdateIncentiveLimitations.Text = _selectedIncentiveForUpdate.Limitations;

                // Set incentive type checkboxes based on current selections
                SetUpdateIncentiveTypeCheckboxes(_selectedIncentiveForUpdate.IncentiveTypesDisplay);

                // Show edit form
                grpUpdateIncentiveForm.Visibility = Visibility.Visible;
                statMessage.Content = $"Editing Incentive ID: {_selectedIncentiveForUpdate.IncentiveID}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading incentive: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadUpdateIncentiveTypes()
        {
            LoadIncentiveTypesToListBox(lstUpdateIncentiveTypes, _selectedUpdateIncentiveTypeIDs);
        }

        private void SetUpdateIncentiveTypeCheckboxes(string incentiveTypesDisplay)
        {
            _selectedUpdateIncentiveTypeIDs.Clear();

            if (string.IsNullOrWhiteSpace(incentiveTypesDisplay))
            {
                return;
            }

            // Parse the display string to get individual type descriptions
            var typeDescriptions = incentiveTypesDisplay.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            // Need to wait for the UI to render the checkboxes
            lstUpdateIncentiveTypes.UpdateLayout();

            // Iterate through the items and check the appropriate checkboxes
            foreach (var item in lstUpdateIncentiveTypes.Items)
            {
                var incentiveType = item as IncentiveType;
                if (incentiveType != null)
                {
                    var container = lstUpdateIncentiveTypes.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    if (container != null)
                    {
                        var checkBox = FindVisualChild<CheckBox>(container);
                        if (checkBox != null)
                        {
                            // Check if this type's description is in the display string
                            bool shouldBeChecked = typeDescriptions.Any(td =>
                                td.Trim().Equals(incentiveType.IncentiveTypeDescription, StringComparison.OrdinalIgnoreCase) ||
                                td.Trim().Equals(incentiveType.IncentiveTypeID, StringComparison.OrdinalIgnoreCase));

                            checkBox.IsChecked = shouldBeChecked;

                            if (shouldBeChecked && !_selectedUpdateIncentiveTypeIDs.Contains(incentiveType.IncentiveTypeID))
                            {
                                _selectedUpdateIncentiveTypeIDs.Add(incentiveType.IncentiveTypeID);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateIncentiveTypeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            HandleIncentiveTypeCheckBoxChanged(sender as CheckBox, _selectedUpdateIncentiveTypeIDs);
        }

        private void btnUpdateIncentiveSave_Click(object sender, RoutedEventArgs e)
        {
            // Validation - Incentive Selected
            if (_selectedIncentiveForUpdate == null)
            {
                ShowValidationError("Please search and select an incentive first.");
                return;
            }

            // Validation - Amount
            if (string.IsNullOrWhiteSpace(txtUpdateIncentiveAmount.Text))
            {
                ShowValidationError("Incentive Amount is required.", txtUpdateIncentiveAmount);
                return;
            }

            if (!decimal.TryParse(txtUpdateIncentiveAmount.Text.Trim(), out decimal amount) || amount < 0)
            {
                ShowValidationError("Please enter a valid positive number for Incentive Amount.", txtUpdateIncentiveAmount);
                txtUpdateIncentiveAmount.SelectAll();
                return;
            }

            // Validation - Description
            if (string.IsNullOrWhiteSpace(txtUpdateIncentiveDescription.Text))
            {
                ShowValidationError("Incentive Description is required.", txtUpdateIncentiveDescription);
                return;
            }

            // Validation - Start Date
            if (!dpUpdateIncentiveStartDate.SelectedDate.HasValue)
            {
                ShowValidationError("Start Date is required.", dpUpdateIncentiveStartDate);
                return;
            }

            // Validation - End Date (if provided, must be after Start Date)
            if (dpUpdateIncentiveEndDate.SelectedDate.HasValue &&
                dpUpdateIncentiveEndDate.SelectedDate.Value < dpUpdateIncentiveStartDate.SelectedDate.Value)
            {
                ShowValidationError("End Date must be after Start Date.", dpUpdateIncentiveEndDate);
                return;
            }

            // Validation - Incentive Types
            if (_selectedUpdateIncentiveTypeIDs.Count == 0)
            {
                ShowValidationError("Please select at least one Incentive Type.", lstUpdateIncentiveTypes);
                return;
            }

            try
            {
                // Update the incentive object with form values
                _selectedIncentiveForUpdate.IncentiveAmount = amount;
                _selectedIncentiveForUpdate.IsPercentage = chkUpdateIncentiveIsPercentage.IsChecked ?? false;
                _selectedIncentiveForUpdate.IncentiveDescription = txtUpdateIncentiveDescription.Text.Trim();
                _selectedIncentiveForUpdate.StartDate = dpUpdateIncentiveStartDate.SelectedDate.Value;
                _selectedIncentiveForUpdate.EndDate = dpUpdateIncentiveEndDate.SelectedDate;
                _selectedIncentiveForUpdate.Limitations = txtUpdateIncentiveLimitations.Text.Trim();

                // Save to database
                bool success = _incentiveManager.UpdateIncentive(_selectedIncentiveForUpdate, _selectedUpdateIncentiveTypeIDs);

                if (success)
                {
                    MessageBox.Show("Incentive updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    int updatedIncentiveID = _selectedIncentiveForUpdate.IncentiveID;

                    // Refresh search results
                    btnUpdateIncentiveSearch_Click(sender, e);

                    statMessage.Content = $"Incentive ID {updatedIncentiveID} updated successfully";
                }
                else
                {
                    MessageBox.Show("Failed to update incentive. The incentive may have been deleted.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nDetails: " + ex.InnerException.Message;
                }

                MessageBox.Show($"Error updating incentive:\n\n{errorMessage}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnUpdateIncentiveCancel_Click(object sender, RoutedEventArgs e)
        {
            grpUpdateIncentiveForm.Visibility = Visibility.Collapsed;
            _selectedIncentiveForUpdate = null;
            _selectedUpdateIncentiveTypeIDs.Clear();
            statMessage.Content = "Update cancelled. Select an incentive to edit.";
        }

        private void LoadAccountStatusFilter()
        {
            try
            {
                var accountStatuses = _lookupManager.GetAllAccountStatuses();

                // Create a list with "All" option first
                var filterList = new List<string> { "All" };
                filterList.AddRange(accountStatuses.Select(a => a.AccountStatusID));

                cmbUserAccountStatusFilter.ItemsSource = filterList;
                cmbUserAccountStatusFilter.SelectedIndex = 0;  // Default to "All"
            }
            catch (Exception ex)
            {
                // If we can't load account statuses, just show "All" option
                cmbUserAccountStatusFilter.ItemsSource = new List<string> { "All" };
                cmbUserAccountStatusFilter.SelectedIndex = 0;
                System.Diagnostics.Debug.WriteLine($"Error loading account status filter: {ex.Message}");
            }
        }

        private void cmbUserAccountStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Prevent firing during initialization
            if (cmbUserAccountStatusFilter.SelectedItem == null)
                return;

            // Only refresh if we're on the Admin tab
            if (tabSetMain.SelectedItem == tabAdmin)
            {
                RefreshUserGridWithFilter();
            }
        }

        private void RefreshUserGridWithFilter()
        {
            try
            {
                string selectedStatus = cmbUserAccountStatusFilter.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selectedStatus) || selectedStatus == "All")
                {
                    // Load all users by querying each status
                    var allUsers = new List<User>();

                    try
                    {
                        var accountStatuses = _lookupManager.GetAllAccountStatuses();
                        foreach (var status in accountStatuses)
                        {
                            var usersForStatus = _userManager.GetUsersByActive(status.AccountStatusID);
                            allUsers.AddRange(usersForStatus);
                        }
                    }
                    catch
                    {
                        // Fallback: just load Active users
                        allUsers = _userManager.GetUsersByActive("Active");
                    }

                    datUsers.ItemsSource = allUsers.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
                    statMessage.Content = $"Displaying {allUsers.Count} users (all statuses).";
                }
                else
                {
                    var users = _userManager.GetUsersByActive(selectedStatus);
                    datUsers.ItemsSource = users;
                    statMessage.Content = $"Displaying {users.Count} users with status '{selectedStatus}'.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            RefreshUserGridWithFilter();
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addWindow = new AddEditUserWindow(_userManager, _lookupManager);

                if (addWindow.ShowDialog() == true)
                {
                    RefreshUserGrid();
                    statMessage.Content = "User added successfully.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add User window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (datUsers.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to edit.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            try
            {
                User selectedUser = (User)datUsers.SelectedItem;
                // Pass true for editMode to go directly to Edit mode
                var editWindow = new AddEditUserWindow(_userManager, _lookupManager, selectedUser, true);
                if (editWindow.ShowDialog() == true)
                {
                    RefreshUserGrid();
                    statMessage.Content = "User updated successfully.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Edit User window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnViewUser_Click(object sender, RoutedEventArgs e)
        {
            if (datUsers.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to view.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            try
            {
                User selectedUser = (User)datUsers.SelectedItem;
                // Pass false for editMode (or just use the 3-parameter constructor) for View mode
                var viewWindow = new AddEditUserWindow(_userManager, _lookupManager, selectedUser, false);
                viewWindow.ShowDialog();
                RefreshUserGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening View User window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void datUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datUsers.SelectedItem != null)
            {
                var user = (User)datUsers.SelectedItem;
                // Double-click opens View mode (user can click Edit to switch)
                var viewWindow = new AddEditUserWindow(_userManager, _lookupManager, user, false);
                if (viewWindow.ShowDialog() == true)
                {
                    RefreshUserGrid();
                }
            }
        }

        private void datUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datUsers.SelectedItem != null)
            {
                User user = (User)datUsers.SelectedItem;
                System.Diagnostics.Debug.WriteLine($"Selected: {user.FirstName} {user.LastName}");
                statMessage.Content = $"Selected: {user.FirstName} {user.LastName}";
            }
        }
    }
}