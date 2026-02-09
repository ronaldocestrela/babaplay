namespace Domain.Entities;

public class Association
{
    public string Id { get; set; } = new Guid().ToString();
    public required string Name { get; set; }
    public DateTime EstablishedDate { get; set; }
}
