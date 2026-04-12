namespace HandyRank.Models;

public class HandymanProfile
{
    public const int MaxSkillsLength = 1000;
    public const int MaxRankLength = 100;

    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Skills { get; set; } = string.Empty;
    public int XP { get; set; }
    public string Rank { get; set; } = "New";
}
