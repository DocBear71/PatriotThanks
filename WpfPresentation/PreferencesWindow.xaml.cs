using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfPresentation
{
    public partial class PreferencesWindow : Window
    {
        private bool _isDirty = false;
        private bool _isLoading = false;

        public PreferencesWindow()
        {
            InitializeComponent();
            LoadPreferences();
            UpdateThemePreview();
        }

        private void LoadPreferences()
        {
            _isLoading = true;

            var preferences = UserPreferences.Load();
            ApplyPreferencesToUI(preferences);

            _isLoading = false;
            _isDirty = false;
        }

        private void ApplyPreferencesToUI(UserPreferences preferences)
        {
            // Display Settings
            sldFontSize.Value = preferences.FontSize;
            lblFontSizeValue.Text = $"{preferences.FontSize}pt";
            cboTheme.SelectedIndex = preferences.ThemeIndex;
            chkShowImages.IsChecked = preferences.ShowImages;
            chkCompactView.IsChecked = preferences.CompactView;

            // Search Defaults - find the state by tag
            cboDefaultState.SelectedIndex = 0; // Default to "No Default"
            if (!string.IsNullOrEmpty(preferences.DefaultStateName))
            {
                for (int i = 0; i < cboDefaultState.Items.Count; i++)
                {
                    if (cboDefaultState.Items[i] is ComboBoxItem item &&
                        item.Tag?.ToString() == preferences.DefaultStateName)
                    {
                        cboDefaultState.SelectedIndex = i;
                        break;
                    }
                }
            }

            sldSearchRadius.Value = preferences.SearchRadius;
            lblSearchRadiusValue.Text = $"{preferences.SearchRadius} mi";
            cboResultsPerPage.SelectedIndex = preferences.ResultsPerPageIndex;

            // Remember Settings
            chkRememberLastSearch.IsChecked = preferences.RememberLastSearch;
            chkRememberWindowPosition.IsChecked = preferences.RememberWindowPosition;
            chkRememberLastTab.IsChecked = preferences.RememberLastTab;

            // Notifications
            chkNotifyNewBusinesses.IsChecked = preferences.NotifyNewBusinesses;
            chkNotifyNewIncentives.IsChecked = preferences.NotifyNewIncentives;
            chkNotifyExpiringIncentives.IsChecked = preferences.NotifyExpiringIncentives;

            UpdateThemePreview();
        }

        private UserPreferences CollectPreferencesFromUI()
        {
            // Get the selected state name from the Tag
            string defaultStateName = "";
            if (cboDefaultState.SelectedItem is ComboBoxItem selectedState && selectedState.Tag != null)
            {
                defaultStateName = selectedState.Tag.ToString();
            }

            // Get theme name from tag
            string themeName = "Light";
            if (cboTheme.SelectedItem is ComboBoxItem selectedTheme && selectedTheme.Tag != null)
            {
                themeName = selectedTheme.Tag.ToString();
            }

            return new UserPreferences
            {
                // Display Settings
                FontSize = (int)sldFontSize.Value,
                ThemeIndex = cboTheme.SelectedIndex,
                ThemeName = themeName,
                ShowImages = chkShowImages.IsChecked ?? true,
                CompactView = chkCompactView.IsChecked ?? false,

                // Search Defaults
                DefaultStateName = defaultStateName,
                SearchRadius = (int)sldSearchRadius.Value,
                ResultsPerPageIndex = cboResultsPerPage.SelectedIndex,

                // Remember Settings
                RememberLastSearch = chkRememberLastSearch.IsChecked ?? true,
                RememberWindowPosition = chkRememberWindowPosition.IsChecked ?? true,
                RememberLastTab = chkRememberLastTab.IsChecked ?? false,

                // Notifications
                NotifyNewBusinesses = chkNotifyNewBusinesses.IsChecked ?? true,
                NotifyNewIncentives = chkNotifyNewIncentives.IsChecked ?? true,
                NotifyExpiringIncentives = chkNotifyExpiringIncentives.IsChecked ?? false
            };
        }

        private void UpdateThemePreview()
        {
            if (cboTheme?.SelectedItem == null || previewBg == null) return;

            string theme = (cboTheme.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Light";

            switch (theme)
            {
                case "Dark":
                    previewBg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                    previewAccent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D6EFD"));
                    previewText.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                    break;
                case "HighContrast":
                    previewBg.Background = new SolidColorBrush(Colors.Black);
                    previewAccent.Background = new SolidColorBrush(Colors.Cyan);
                    previewText.Background = new SolidColorBrush(Colors.White);
                    break;
                case "Patriot":
                    previewBg.Background = new SolidColorBrush(Colors.White);
                    previewAccent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#002868"));
                    previewText.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF0A30"));
                    break;
                default: // Light
                    previewBg.Background = new SolidColorBrush(Colors.White);
                    previewAccent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007BFF"));
                    previewText.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212529"));
                    break;
            }
        }

        private void sldFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lblFontSizeValue != null)
            {
                lblFontSizeValue.Text = $"{(int)e.NewValue}pt";
                if (!_isLoading) _isDirty = true;
            }
        }

        private void sldSearchRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lblSearchRadiusValue != null)
            {
                lblSearchRadiusValue.Text = $"{(int)e.NewValue} mi";
                if (!_isLoading) _isDirty = true;
            }
        }

        private void cboTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateThemePreview();
            if (!_isLoading) _isDirty = true;
        }

        private void btnResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all preferences to their default values?",
                "Reset Preferences",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _isLoading = true;
                ApplyPreferencesToUI(new UserPreferences());
                _isLoading = false;
                _isDirty = true;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var preferences = CollectPreferencesFromUI();
                preferences.Save();

                MessageBox.Show(
                    "Preferences saved successfully!\n\n" +
                    "Settings will be applied:\n" +
                    "• Theme and font size - on next app restart\n" +
                    "• Search defaults - immediately\n" +
                    "• Window position - on next close/open",
                    "Preferences Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving preferences: {ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isDirty)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to cancel?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            this.DialogResult = false;
            this.Close();
        }
    }

    public class UserPreferences
    {
        // Current user ID for per-user preferences
        private static int? _currentUserID = null;

        /// <summary>
        /// Sets the current user ID for per-user preferences.
        /// Call this when a user logs in.
        /// </summary>
        public static void SetCurrentUser(int userID)
        {
            _currentUserID = userID;
        }

        /// <summary>
        /// Clears the current user ID.
        /// Call this when a user logs out.
        /// </summary>
        public static void ClearCurrentUser()
        {
            _currentUserID = null;
        }

        /// <summary>
        /// Gets the current user ID, or null if no user is logged in.
        /// </summary>
        public static int? CurrentUserID => _currentUserID;

        // Display Settings
        public int FontSize { get; set; } = 12;
        public int ThemeIndex { get; set; } = 0;
        public string ThemeName { get; set; } = "Light";
        public bool ShowImages { get; set; } = true;
        public bool CompactView { get; set; } = false;

        // Search Defaults
        public string DefaultStateName { get; set; } = "";
        public int SearchRadius { get; set; } = 25;
        public int ResultsPerPageIndex { get; set; } = 1;

        // Remember Settings
        public bool RememberLastSearch { get; set; } = true;
        public bool RememberWindowPosition { get; set; } = true;
        public bool RememberLastTab { get; set; } = false;

        // Notifications
        public bool NotifyNewBusinesses { get; set; } = true;
        public bool NotifyNewIncentives { get; set; } = true;
        public bool NotifyExpiringIncentives { get; set; } = false;

        // Last Search Criteria (for RememberLastSearch feature)
        public string LastSearchBusinessName { get; set; } = "";
        public string LastSearchCity { get; set; } = "";
        public string LastSearchState { get; set; } = "";
        public string LastSearchZip { get; set; } = "";

        // Window Position (for RememberWindowPosition feature)
        public double WindowLeft { get; set; } = -1;
        public double WindowTop { get; set; } = -1;
        public double WindowWidth { get; set; } = 1100;
        public double WindowHeight { get; set; } = 700;
        public bool WindowMaximized { get; set; } = false;

        // Last Tab (for RememberLastTab feature)
        public int LastTabIndex { get; set; } = 0;

        private static readonly string PreferencesDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PatriotThanks");

        /// <summary>
        /// Gets the preferences file path for the current user.
        /// Returns user-specific file if logged in, or default file for guests.
        /// </summary>
        private static string GetPreferencesFilePath()
        {
            string fileName = _currentUserID.HasValue
                ? $"preferences_user_{_currentUserID.Value}.json"
                : "preferences_guest.json";
            return Path.Combine(PreferencesDirectory, fileName);
        }

        public static UserPreferences Load()
        {
            try
            {
                string filePath = GetPreferencesFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var prefs = JsonSerializer.Deserialize<UserPreferences>(json);
                    return prefs ?? new UserPreferences();
                }
            }
            catch (Exception)
            {
                // If there's any error loading, return defaults
            }

            return new UserPreferences();
        }

        public void Save()
        {
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(PreferencesDirectory))
                {
                    Directory.CreateDirectory(PreferencesDirectory);
                }

                // Serialize and save to user-specific file
                string filePath = GetPreferencesFilePath();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save preferences: {ex.Message}", ex);
            }
        }

        public static string GetPreferencesPath()
        {
            return GetPreferencesFilePath();
        }
    }
}