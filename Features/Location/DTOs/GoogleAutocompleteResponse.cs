namespace HandyRank.Features.Location.DTOs;

public class GoogleAutocompleteResponse
{
    public List<Prediction> predictions { get; set; } = new();
}

public class GooglePlaceDetailsResponse
{
    public Result result { get; set; } = new();
}

public class Result
{
    public Geometry geometry { get; set; } = new();
}

public class Geometry
{
    public Location location { get; set; } = new();
}

public class Location
{
    public double lat { get; set; }
    public double lng { get; set; }
}