public class ApplicantDto
{
    public int ProfessionalId { get; set; }
    public string Name { get; set; } = "";
    public string? Bio { get; set; }
    public string? Skills { get; set; }
    public DateTime AppliedAt { get; set; }
}