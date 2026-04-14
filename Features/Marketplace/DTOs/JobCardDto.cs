public class JobCardDto
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    public string Category { get; set; } = "";
    public string Location { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public ServiceRequestStatus Status { get; set; }

    public bool HasApplied { get; set; }

    public int? Rating { get; set; }
    public string? CommentPreview { get; set; }
}