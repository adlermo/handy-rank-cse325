using HandyRank.Features.Location.Services;

namespace HandyRank.Endpoints;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var location = app.MapGroup("/api/location");

        location.MapGet("/autocomplete", async (
            string input,
            LocationService service) =>
        {
            var result = await service.Autocomplete(input);
            return Results.Ok(result);
        });

        location.MapGet("/details", async (
            string placeId,
            LocationService service) =>
        {
            var coords = await service.GetCoordinates(placeId);

            return coords is null
                ? Results.NotFound()
                : Results.Ok(coords);
        });

        return app;
    }
}