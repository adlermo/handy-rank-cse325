namespace HandyRank.Models;

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public int? HandymanId { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Pending;
}
