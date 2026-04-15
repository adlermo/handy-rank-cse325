using HandyRank.Domain.Enums;

namespace HandyRank.Models;

public class User
{
    public const int MaxNameLength = 100;
    public const int MaxBioLength = 2000;
    public const int MaxLocationLength = 200;

    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;

    public int Points { get; set; } = 0;

    public HandymanProfile? HandymanProfile { get; set; }
}
