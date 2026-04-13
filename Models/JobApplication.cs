using HandyRank.Models;

public class JobApplication
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }
    public ServiceRequest ServiceRequest { get; set; } = null!;

    public int ProfessionalId { get; set; }
    public User Professional { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}