public class CreateServiceRequestDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public int CategoryId { get; set; }
}