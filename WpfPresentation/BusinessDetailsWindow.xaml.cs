using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DataDomain;
using LogicLayerInterfaces;

namespace WpfPresentation
{
    /// <summary>
    /// Interaction logic for BusinessDetailsWindow.xaml
    /// </summary>
    public partial class BusinessDetailsWindow : Window
    {
        private Business _business;
        private IIncentiveManager _incentiveManager;

        // Properties to indicate which action the user selected
        public bool EditBusinessRequested { get; private set; } = false;
        public bool ViewIncentivesRequested { get; private set; } = false;
        public bool AddIncentiveRequested { get; private set; } = false;

        public BusinessDetailsWindow(Business business, IIncentiveManager incentiveManager)
        {
            InitializeComponent();

            _business = business;
            _incentiveManager = incentiveManager;

            PopulateBusinessDetails();
        }

        private void PopulateBusinessDetails()
        {
            // Business name header
            txtBusinessName.Text = _business.BusinessName;

            // Business type
            txtBusinessType.Text = string.IsNullOrWhiteSpace(_business.BusinessType)
                ? _business.BusinessTypeID
                : _business.BusinessType;

            // Phone
            txtPhone.Text = string.IsNullOrWhiteSpace(_business.Phone)
                ? "Not on file"
                : _business.Phone;

            // Status
            txtStatus.Text = _business.IsActive ? "Active" : "Inactive";

            // Address
            txtStreetAddress.Text = _business.StreetAddress;

            if (!string.IsNullOrWhiteSpace(_business.Address2))
            {
                txtAddress2.Text = _business.Address2;
                txtAddress2.Visibility = Visibility.Visible;
            }
            else
            {
                txtAddress2.Visibility = Visibility.Collapsed;
            }

            txtCityStateZip.Text = $"{_business.City}, {_business.StateID} {_business.Zip}";

            if (!string.IsNullOrWhiteSpace(_business.LocationName))
            {
                txtLocationName.Text = $"Location: {_business.LocationName}";
                txtLocationName.Visibility = Visibility.Visible;
            }
            else
            {
                txtLocationName.Visibility = Visibility.Collapsed;
            }

            // Load incentive summary
            LoadIncentiveSummary();
        }

        private void LoadIncentiveSummary()
        {
            try
            {
                // Get incentives for this business
                var searchCriteria = new IncentiveSearchCriteria
                {
                    BusinessID = _business.BusinessID
                };

                var incentives = _incentiveManager.SearchIncentives(searchCriteria);

                if (incentives == null || incentives.Count == 0)
                {
                    txtIncentiveSummary.Text = "No incentives on file for this business.";
                }
                else
                {
                    int activeCount = incentives.Count(i => i.IsCurrentlyActive);
                    int totalCount = incentives.Count;

                    StringBuilder summary = new StringBuilder();
                    summary.AppendLine($"Total Incentives: {totalCount}");
                    summary.AppendLine($"Currently Active: {activeCount}");

                    if (activeCount > 0)
                    {
                        summary.AppendLine();
                        summary.AppendLine("Active Offers:");
                        foreach (var incentive in incentives.Where(i => i.IsCurrentlyActive).Take(3))
                        {
                            summary.AppendLine($"  • {incentive.FormattedAmount} - {TruncateText(incentive.IncentiveDescription, 40)}");
                        }

                        if (activeCount > 3)
                        {
                            summary.AppendLine($"  ... and {activeCount - 3} more");
                        }
                    }

                    txtIncentiveSummary.Text = summary.ToString();
                }
            }
            catch (Exception ex)
            {
                txtIncentiveSummary.Text = $"Unable to load incentives: {ex.Message}";
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }
            return text.Substring(0, maxLength) + "...";
        }

        private void btnEditBusiness_Click(object sender, RoutedEventArgs e)
        {
            EditBusinessRequested = true;
            this.DialogResult = true;
            this.Close();
        }

        private void btnViewIncentives_Click(object sender, RoutedEventArgs e)
        {
            ViewIncentivesRequested = true;
            this.DialogResult = true;
            this.Close();
        }

        private void btnAddIncentive_Click(object sender, RoutedEventArgs e)
        {
            AddIncentiveRequested = true;
            this.DialogResult = true;
            this.Close();
        }

        private void btnViewMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mapWindow = new BusinessMapWindow(_business);
                mapWindow.Owner = this;
                mapWindow.ShowDialog();
                // Don't close this window - user may want to do something else after viewing map
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening map: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}