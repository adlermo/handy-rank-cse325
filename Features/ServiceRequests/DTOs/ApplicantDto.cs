public class ApplicantDto
{
    public int ProfessionalId { get; set; }
    public string Name { get; set; } = "";
    public string? Bio { get; set; }
    public string? Skills { get; set; }
    public string Rank { get; set; } = "";
    public int TotalJobsCompleted { get; set; }
    public double Rating { get; set; }
    public DateTime AppliedAt { get; set; }
}