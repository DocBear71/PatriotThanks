using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DataDomain;
using LogicLayer;

namespace WpfPresentation
{
    /// <summary>
    /// Interaction logic for BusinessMapWindow.xaml
    /// Shows a static map of a business location using LocationIQ
    /// </summary>
    public partial class BusinessMapWindow : Window
    {
        private Business _business;
        private LocationIQService _locationService;
        private GeocodingResult _geocodingResult;
        private int _currentZoom = 15;
        private string _currentMapStyle = "streets";

        // TODO: Move this to a configuration file or app settings
        // Get your free API key at: https://locationiq.com/
        private const string LOCATIONIQ_API_KEY = "pk.ebd85d6d18868c4c3b3f430f6e1ea34e";

        public BusinessMapWindow(Business business)
        {
            InitializeComponent();

            _business = business;
            _locationService = new LocationIQService(LOCATIONIQ_API_KEY);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set header information
            txtBusinessName.Text = _business.BusinessName;
            txtAddress.Text = _business.FullAddress;

            // Disable zoom controls until map loads
            SetControlsEnabled(false);

            await LoadMapAsync();
        }

        private async System.Threading.Tasks.Task LoadMapAsync()
        {
            try
            {
                // Show loading state
                ShowLoading();

                // First, geocode the address if we haven't already
                if (_geocodingResult == null)
                {
                    _geocodingResult = await _locationService.GeocodeAddressAsync(
                        _business.StreetAddress,
                        _business.City,
                        _business.StateID,
                        _business.Zip
                    );

                    if (_geocodingResult == null)
                    {
                        ShowError("Could not find the location for this address.\n\nPlease verify the address is correct.");
                        return;
                    }

                    Debug.WriteLine($"Geocoded to: {_geocodingResult.Latitude}, {_geocodingResult.Longitude}");
                }

                // Get the static map image
                byte[] imageData = await _locationService.GetStaticMapImageAsync(
                    _geocodingResult.Latitude,
                    _geocodingResult.Longitude,
                    width: 600,
                    height: 400,
                    zoom: _currentZoom,
                    mapType: _currentMapStyle
                );

                if (imageData == null || imageData.Length == 0)
                {
                    ShowError("Could not load the map image.\n\nPlease check your internet connection and try again.");
                    return;
                }

                // Convert byte array to BitmapImage
                BitmapImage bitmap = new BitmapImage();
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Important for cross-thread access
                }

                // Display the map
                imgMap.Source = bitmap;
                ShowMap();
                SetControlsEnabled(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading map: {ex.Message}");
                ShowError($"An error occurred while loading the map:\n\n{ex.Message}");
            }
        }

        private void ShowLoading()
        {
            pnlLoading.Visibility = Visibility.Visible;
            pnlError.Visibility = Visibility.Collapsed;
            imgMap.Visibility = Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            pnlLoading.Visibility = Visibility.Collapsed;
            pnlError.Visibility = Visibility.Visible;
            imgMap.Visibility = Visibility.Collapsed;
            txtErrorMessage.Text = message;
            SetControlsEnabled(false);
        }

        private void ShowMap()
        {
            pnlLoading.Visibility = Visibility.Collapsed;
            pnlError.Visibility = Visibility.Collapsed;
            imgMap.Visibility = Visibility.Visible;
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnZoomIn.IsEnabled = enabled;
            btnZoomOut.IsEnabled = enabled;
            cmbMapStyle.IsEnabled = enabled;
            btnOpenInBrowser.IsEnabled = enabled && _geocodingResult != null;
        }

        private async void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentZoom < 18)
            {
                _currentZoom++;
                txtZoomLevel.Text = _currentZoom.ToString();
                await LoadMapAsync();
            }
        }

        private async void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (_currentZoom > 1)
            {
                _currentZoom--;
                txtZoomLevel.Text = _currentZoom.ToString();
                await LoadMapAsync();
            }
        }

        private async void cmbMapStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMapStyle.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag != null &&
                _geocodingResult != null) // Only reload if we have coordinates
            {
                string newStyle = selectedItem.Tag.ToString();
                if (newStyle != _currentMapStyle)
                {
                    _currentMapStyle = newStyle;
                    await LoadMapAsync();
                }
            }
        }

        private async void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            await LoadMapAsync();
        }

        private void btnOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (_geocodingResult != null)
            {
                // Open OpenStreetMap in the default browser
                string url = $"https://www.openstreetmap.org/?mlat={_geocodingResult.Latitude}&mlon={_geocodingResult.Longitude}#map={_currentZoom}/{_geocodingResult.Latitude}/{_geocodingResult.Longitude}";

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open browser: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _locationService?.Dispose();
        }
    }
}