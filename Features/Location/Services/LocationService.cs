using System.Text.Json;
using HandyRank.Features.Location.DTOs;

namespace HandyRank.Features.Location.Services;

public class LocationService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    public LocationService(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _config = config;
    }

    public async Task<List<LocationSuggestion>> Autocomplete(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 3)
            return new();

        var apiKey = _config["GoogleMaps:ApiKey"];
        var client = _httpFactory.CreateClient();

        var url =
            $"https://maps.googleapis.com/maps/api/place/autocomplete/json" +
            $"?input={Uri.EscapeDataString(input)}" +
            $"&types=geocode" +
            $"&key={apiKey}";

        var response = await client.GetStringAsync(url);
        Console.WriteLine(response);

        var parsed = JsonSerializer.Deserialize<GoogleAutocompleteResponse>(response);

        return parsed?.predictions?
            .Select(p => new LocationSuggestion
            {
                Description = p.description,
                PlaceId = p.place_id
            })
            .ToList() ?? new();
    }

    public async Task<(double lat, double lng)?> GetCoordinates(string placeId)
    {
        var apiKey = _config["GoogleMaps:ApiKey"];
        var client = _httpFactory.CreateClient();

        var url =
            $"https://maps.googleapis.com/maps/api/place/details/json" +
            $"?place_id={placeId}" +
            $"&fields=geometry" +
            $"&key={apiKey}";

        var response = await client.GetStringAsync(url);

        var parsed = JsonSerializer.Deserialize<GooglePlaceDetailsResponse>(response);

        var loc = parsed?.result?.geometry?.location;

        if (loc == null) return null;

        return (loc.lat, loc.lng);
    }
}