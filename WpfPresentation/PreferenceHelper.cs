using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfPresentation
{
    public static class PreferencesHelper
    {
        public static void ApplyAllPreferences(
            Window window,
            ComboBox stateComboBox = null,
            TextBox businessNameTextBox = null,
            TextBox cityTextBox = null,
            TextBox zipTextBox = null,
            TabControl tabControl = null)
        {
            var prefs = UserPreferences.Load();

            // Apply window position
            ApplyWindowPosition(window, prefs);

            // Apply theme
            ApplyTheme(window, prefs);

            // Apply font size
            ApplyFontSize(window, prefs);

            // Apply default state to search
            if (stateComboBox != null)
            {
                ApplyDefaultState(stateComboBox, prefs);
            }

            // Apply last search criteria
            if (prefs.RememberLastSearch)
            {
                ApplyLastSearch(businessNameTextBox, cityTextBox, stateComboBox, zipTextBox, prefs);
            }

            // Apply last tab
            if (prefs.RememberLastTab && tabControl != null)
            {
                ApplyLastTab(tabControl, prefs);
            }
        }

        public static void ApplyWindowPosition(Window window, UserPreferences prefs = null)
        {
            if (prefs == null) prefs = UserPreferences.Load();

            if (!prefs.RememberWindowPosition) return;

            try
            {
                // Check if we have valid saved position
                if (prefs.WindowLeft >= 0 && prefs.WindowTop >= 0)
                {
                    // Make sure position is on screen
                    var screenWidth = SystemParameters.VirtualScreenWidth;
                    var screenHeight = SystemParameters.VirtualScreenHeight;

                    if (prefs.WindowLeft < screenWidth && prefs.WindowTop < screenHeight)
                    {
                        window.Left = prefs.WindowLeft;
                        window.Top = prefs.WindowTop;
                    }
                }

                // Apply size
                if (prefs.WindowWidth > 0 && prefs.WindowHeight > 0)
                {
                    window.Width = prefs.WindowWidth;
                    window.Height = prefs.WindowHeight;
                }

                // Apply maximized state
                if (prefs.WindowMaximized)
                {
                    window.WindowState = WindowState.Maximized;
                }
            }
            catch (Exception)
            {
                // Silently fail - window will just use defaults
            }
        }

        public static void SaveWindowPosition(Window window)
        {
            var prefs = UserPreferences.Load();

            if (!prefs.RememberWindowPosition) return;

            try
            {
                prefs.WindowMaximized = window.WindowState == WindowState.Maximized;

                // Only save position if not maximized
                if (!prefs.WindowMaximized)
                {
                    prefs.WindowLeft = window.Left;
                    prefs.WindowTop = window.Top;
                    prefs.WindowWidth = window.Width;
                    prefs.WindowHeight = window.Height;
                }

                prefs.Save();
            }
            catch (Exception)
            {
                // Silently fail
            }
        }

        public static void ApplyTheme(Window window, UserPreferences prefs = null)
        {
            if (prefs == null) prefs = UserPreferences.Load();

            try
            {
                // Get theme resource dictionary path
                string themePath = prefs.ThemeName switch
                {
                    "Dark" => "Themes/DarkTheme.xaml",
                    "HighContrast" => "Themes/HighContrastTheme.xaml",
                    "Patriot" => "Themes/PatriotTheme.xaml",
                    _ => "Themes/LightTheme.xaml"
                };

                // Try to load and apply theme
                var themeUri = new Uri(themePath, UriKind.Relative);

                // Check if theme resource exists
                try
                {
                    var themeDictionary = new ResourceDictionary { Source = themeUri };

                    // Remove existing theme dictionaries
                    var toRemove = new System.Collections.Generic.List<ResourceDictionary>();
                    foreach (var dict in Application.Current.Resources.MergedDictionaries)
                    {
                        if (dict.Source != null && dict.Source.OriginalString.Contains("Theme"))
                        {
                            toRemove.Add(dict);
                        }
                    }
                    foreach (var dict in toRemove)
                    {
                        Application.Current.Resources.MergedDictionaries.Remove(dict);
                    }

                    // Add new theme
                    Application.Current.Resources.MergedDictionaries.Add(themeDictionary);

                    // Apply key colors directly to window if resources are found
                    ApplyThemeColorsDirectly(window, prefs.ThemeName);
                }
                catch (Exception)
                {
                    // Theme file doesn't exist yet, apply colors directly
                    ApplyThemeColorsDirectly(window, prefs.ThemeName);
                }
            }
            catch (Exception)
            {
                // Silently fail - will use default colors
            }
        }

        private static void ApplyThemeColorsDirectly(Window window, string themeName)
        {
            Color bgColor, fgColor, accentColor;

            switch (themeName)
            {
                case "Dark":
                    bgColor = (Color)ColorConverter.ConvertFromString("#1E1E1E");
                    fgColor = (Color)ColorConverter.ConvertFromString("#E0E0E0");
                    accentColor = (Color)ColorConverter.ConvertFromString("#0D6EFD");
                    break;
                case "HighContrast":
                    bgColor = Colors.Black;
                    fgColor = Colors.White;
                    accentColor = Colors.Cyan;
                    break;
                case "Patriot":
                    bgColor = Colors.White;
                    fgColor = (Color)ColorConverter.ConvertFromString("#002868");
                    accentColor = (Color)ColorConverter.ConvertFromString("#BF0A30");
                    break;
                default: // Light
                    bgColor = Colors.White;
                    fgColor = (Color)ColorConverter.ConvertFromString("#212529");
                    accentColor = (Color)ColorConverter.ConvertFromString("#007BFF");
                    break;
            }

            // Apply to window if it has a Grid as content
            if (window.Content is Grid mainGrid)
            {
                mainGrid.Background = new SolidColorBrush(bgColor);

                // Apply to all DataGrids
                ApplyToDataGrids(mainGrid, bgColor, fgColor, accentColor, themeName);
            }
        }

        private static void ApplyToDataGrids(DependencyObject parent, Color bgColor, Color fgColor, Color accentColor, string themeName)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is DataGrid dataGrid)
                {
                    dataGrid.Background = new SolidColorBrush(bgColor);
                    dataGrid.Foreground = new SolidColorBrush(fgColor);
                    dataGrid.RowBackground = new SolidColorBrush(bgColor);

                    // Alternate row color
                    Color altColor = themeName switch
                    {
                        "Dark" => (Color)ColorConverter.ConvertFromString("#252526"),
                        "HighContrast" => (Color)ColorConverter.ConvertFromString("#1A1A1A"),
                        "Patriot" => (Color)ColorConverter.ConvertFromString("#F0F4F8"),
                        _ => (Color)ColorConverter.ConvertFromString("#F8F9FA")
                    };
                    dataGrid.AlternatingRowBackground = new SolidColorBrush(altColor);
                }

                // Recurse
                ApplyToDataGrids(child, bgColor, fgColor, accentColor, themeName);
            }
        }

        public static void ApplyFontSize(Window window, UserPreferences prefs = null)
        {
            if (prefs == null) prefs = UserPreferences.Load();

            try
            {
                // Set the base font size for the window
                window.FontSize = prefs.FontSize;

                // Apply to all DataGrids specifically
                if (window.Content is DependencyObject content)
                {
                    ApplyFontSizeToDataGrids(content, prefs.FontSize);
                }
            }
            catch (Exception)
            {
                // Silently fail
            }
        }

        private static void ApplyFontSizeToDataGrids(DependencyObject parent, int fontSize)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is DataGrid dataGrid)
                {
                    dataGrid.FontSize = fontSize;
                }
                else if (child is Control control)
                {
                    // Apply to most controls
                    control.FontSize = fontSize;
                }

                // Recurse
                ApplyFontSizeToDataGrids(child, fontSize);
            }
        }

        public static void ApplyDefaultState(ComboBox stateComboBox, UserPreferences prefs = null)
        {
            if (prefs == null) prefs = UserPreferences.Load();
            if (stateComboBox == null || string.IsNullOrEmpty(prefs.DefaultStateName)) return;

            try
            {
                string searchName = prefs.DefaultStateName.ToLower();

                foreach (var item in stateComboBox.Items)
                {
                    // Handle ComboBoxItems (your format: Content = "IA - Iowa", Tag = "IA")
                    if (item is ComboBoxItem comboItem)
                    {
                        string content = comboItem.Content?.ToString() ?? "";
                        string tag = comboItem.Tag?.ToString() ?? "";

                        // Match if:
                        // 1. Content contains the state name (e.g., "IA - Iowa" contains "Iowa")
                        // 2. Tag matches the state name (for abbreviations)
                        // 3. Exact match on content
                        if (content.ToLower().Contains(searchName) ||
                            tag.Equals(prefs.DefaultStateName, StringComparison.OrdinalIgnoreCase) ||
                            content.Equals(prefs.DefaultStateName, StringComparison.OrdinalIgnoreCase))
                        {
                            stateComboBox.SelectedItem = item;
                            return;
                        }
                    }
                    // Handle if items are State objects (StateID, StateDescription)
                    else
                    {
                        var itemType = item.GetType();
                        var stateDescProp = itemType.GetProperty("StateDescription");
                        var stateIdProp = itemType.GetProperty("StateID");

                        if (stateDescProp != null)
                        {
                            var stateDesc = stateDescProp.GetValue(item)?.ToString() ?? "";
                            var stateId = stateIdProp?.GetValue(item)?.ToString() ?? "";

                            if (stateDesc.Equals(prefs.DefaultStateName, StringComparison.OrdinalIgnoreCase) ||
                                stateId.Equals(prefs.DefaultStateName, StringComparison.OrdinalIgnoreCase))
                            {
                                stateComboBox.SelectedItem = item;
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail
            }
        }

        public static void ApplyLastSearch(
            TextBox businessNameTextBox,
            TextBox cityTextBox,
            ComboBox stateComboBox,
            TextBox zipTextBox,
            UserPreferences prefs = null)
        {
            if (prefs == null) prefs = UserPreferences.Load();
            if (!prefs.RememberLastSearch) return;

            try
            {
                if (businessNameTextBox != null && !string.IsNullOrEmpty(prefs.LastSearchBusinessName))
                {
                    businessNameTextBox.Text = prefs.LastSearchBusinessName;
                }

                if (cityTextBox != null && !string.IsNullOrEmpty(prefs.LastSearchCity))
                {
                    cityTextBox.Text = prefs.LastSearchCity;
                }

                if (zipTextBox != null && !string.IsNullOrEmpty(prefs.LastSearchZip))
                {
                    zipTextBox.Text = prefs.LastSearchZip;
                }

                // Apply last search state (handles ComboBoxItems with "IA - Iowa" format)
                if (stateComboBox != null && !string.IsNullOrEmpty(prefs.LastSearchState))
                {
                    foreach (var item in stateComboBox.Items)
                    {
                        if (item is ComboBoxItem comboItem)
                        {
                            string content = comboItem.Content?.ToString() ?? "";
                            string tag = comboItem.Tag?.ToString() ?? "";

                            // Match by Tag (StateID like "IA") or Content containing state name
                            if (tag.Equals(prefs.LastSearchState, StringComparison.OrdinalIgnoreCase) ||
                                content.ToLower().Contains(prefs.LastSearchState.ToLower()))
                            {
                                stateComboBox.SelectedItem = item;
                                break;
                            }
                        }
                        else
                        {
                            var itemType = item.GetType();
                            var stateDescProp = itemType.GetProperty("StateDescription");
                            var stateIdProp = itemType.GetProperty("StateID");

                            if (stateIdProp != null)
                            {
                                var stateId = stateIdProp.GetValue(item)?.ToString() ?? "";
                                if (stateId.Equals(prefs.LastSearchState, StringComparison.OrdinalIgnoreCase))
                                {
                                    stateComboBox.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail
            }
        }


        public static void SaveLastSearch(
            TextBox businessNameTextBox,
            TextBox cityTextBox,
            ComboBox stateComboBox,
            TextBox zipTextBox)
        {
            var prefs = UserPreferences.Load();
            if (!prefs.RememberLastSearch) return;

            try
            {
                prefs.LastSearchBusinessName = businessNameTextBox?.Text ?? "";
                prefs.LastSearchCity = cityTextBox?.Text ?? "";
                prefs.LastSearchZip = zipTextBox?.Text ?? "";

                // Get state name
                if (stateComboBox?.SelectedItem != null)
                {
                    var item = stateComboBox.SelectedItem;
                    var itemType = item.GetType();
                    var stateNameProp = itemType.GetProperty("StateName");

                    if (stateNameProp != null)
                    {
                        prefs.LastSearchState = stateNameProp.GetValue(item)?.ToString() ?? "";
                    }
                }

                prefs.Save();
            }
            catch (Exception)
            {
                // Silently fail
            }
        }

        public static void ApplyLastTab(TabControl tabControl, UserPreferences prefs = null)
        {
            if (prefs == null) prefs = UserPreferences.Load();
            if (!prefs.RememberLastTab || tabControl == null) return;

            try
            {
                if (prefs.LastTabIndex >= 0 && prefs.LastTabIndex < tabControl.Items.Count)
                {
                    tabControl.SelectedIndex = prefs.LastTabIndex;
                }
            }
            catch (Exception)
            {
                // Silently fail
            }
        }

        public static void SaveLastTab(TabControl tabControl)
        {
            var prefs = UserPreferences.Load();
            if (!prefs.RememberLastTab || tabControl == null) return;

            try
            {
                prefs.LastTabIndex = tabControl.SelectedIndex;
                prefs.Save();
            }
            catch (Exception)
            {
                // Silently fail
            }
        }
    }
}