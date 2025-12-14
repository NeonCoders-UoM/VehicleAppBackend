using System.Net.Http;
using System.Text.Json;

namespace Vpassbackend.Services
{
    public interface IGoogleMapsService
    {
        Task<(double? Latitude, double? Longitude)> GetCoordinatesFromAddressAsync(string address);
    }

    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public GoogleMapsService(IConfiguration configuration, HttpClient httpClient)
        {
            _apiKey = configuration["GoogleMaps:ApiKey"]
                ?? throw new InvalidOperationException("Google Maps API key not configured");
            _httpClient = httpClient;
        }

        public async Task<(double? Latitude, double? Longitude)> GetCoordinatesFromAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return (null, null);
            }

            try
            {
                var encodedAddress = Uri.EscapeDataString(address);
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeocodeResponse>(json);

                if (result?.status == "OK" && result.results?.Length > 0)
                {
                    var location = result.results[0].geometry.location;
                    return (location.lat, location.lng);
                }

                Console.WriteLine($"Geocoding failed for address: {address}. Status: {result?.status}");
                return (null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error geocoding address '{address}': {ex.Message}");
                return (null, null);
            }
        }

        // Response DTOs for Google Geocoding API
        private class GeocodeResponse
        {
            public string? status { get; set; }
            public GeocodeResult[]? results { get; set; }
        }

        private class GeocodeResult
        {
            public Geometry? geometry { get; set; }
        }

        private class Geometry
        {
            public Location? location { get; set; }
        }

        private class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
    }
}
