using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace LogicLayer
{
    public class LocationIQService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        // API endpoints
        private const string GEOCODING_BASE_URL = "https://us1.locationiq.com/v1/search";
        private const string STATIC_MAP_BASE_URL = "https://maps.locationiq.com/v3/staticmap";

        public LocationIQService(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            }

            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<GeocodingResult> GeocodeAddressAsync(string streetAddress, string city, string state, string zip)
        {
            try
            {
                // Build the full address string
                string fullAddress = BuildAddressString(streetAddress, city, state, zip);

                if (string.IsNullOrWhiteSpace(fullAddress))
                {
                    return null;
                }

                // URL encode the address
                string encodedAddress = HttpUtility.UrlEncode(fullAddress);

                // Build the API URL
                string url = $"{GEOCODING_BASE_URL}?key={_apiKey}&q={encodedAddress}&format=json&limit=1";

                // Make the request
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Geocoding API error: {response.StatusCode}");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Parse the JSON response
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    JsonElement root = doc.RootElement;

                    if (root.GetArrayLength() == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Geocoding returned no results");
                        return null;
                    }

                    JsonElement firstResult = root[0];

                    string latStr = firstResult.GetProperty("lat").GetString();
                    string lonStr = firstResult.GetProperty("lon").GetString();
                    string displayName = firstResult.GetProperty("display_name").GetString();

                    if (double.TryParse(latStr, out double lat) && double.TryParse(lonStr, out double lon))
                    {
                        return new GeocodingResult
                        {
                            Latitude = lat,
                            Longitude = lon,
                            DisplayName = displayName
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Geocoding error: {ex.Message}");
                return null;
            }
        }

        public string GetStaticMapUrl(double latitude, double longitude,
            int width = 600, int height = 400, int zoom = 15, string mapType = "streets")
        {
            // Validate parameters
            zoom = Math.Max(1, Math.Min(18, zoom));
            width = Math.Max(100, Math.Min(1280, width));
            height = Math.Max(100, Math.Min(1280, height));

            // Build the static map URL with a marker
            string url = $"{STATIC_MAP_BASE_URL}" +
                         $"?key={_apiKey}" +
                         $"&center={latitude},{longitude}" +
                         $"&zoom={zoom}" +
                         $"&size={width}x{height}" +
                         $"&format=png" +
                         $"&maptype={mapType}" +
                         $"&markers=icon:large-red-cutout|{latitude},{longitude}";

            return url;
        }

        public async Task<byte[]> GetStaticMapImageAsync(double latitude, double longitude,
            int width = 600, int height = 400, int zoom = 15, string mapType = "streets")
        {
            try
            {
                string url = GetStaticMapUrl(latitude, longitude, width, height, zoom, mapType);

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Static map API error: {response.StatusCode}");
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Static map error: {ex.Message}");
                return null;
            }
        }

        private string BuildAddressString(string streetAddress, string city, string state, string zip)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(streetAddress))
                parts.Add(streetAddress.Trim());

            if (!string.IsNullOrWhiteSpace(city))
                parts.Add(city.Trim());

            if (!string.IsNullOrWhiteSpace(state))
                parts.Add(state.Trim());

            if (!string.IsNullOrWhiteSpace(zip))
                parts.Add(zip.Trim());

            // Add USA to improve geocoding accuracy
            parts.Add("USA");

            return string.Join(", ", parts);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class GeocodingResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DisplayName { get; set; }
    }
}