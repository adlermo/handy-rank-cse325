public class Review
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }
    public int CustomerId { get; set; }
    public int ProfessionalId { get; set; }

    public int Rating { get; set; } // 1–5

    public List<ReviewTag> Tags { get; set; } = new();
    public List<ReviewTagLink> ReviewTags { get; set; } = new();

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}