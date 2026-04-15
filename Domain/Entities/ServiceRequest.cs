using HandyRank.Domain.Enums;
using HandyRank.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public JobSize Size { get; set; } = JobSize.Medium;

    public int CategoryId { get; set; }
    public ServiceCategory? Category { get; set; }

    public int CustomerId { get; set; }
    public User? Customer { get; set; }

    public int? ProfessionalId { get; set; }
    public User? Professional { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Open;
}