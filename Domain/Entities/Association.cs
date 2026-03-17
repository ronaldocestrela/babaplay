namespace Domain.Entities;

public class Association : BaseEntity
{
    public required string Name { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string? Statute { get; set; }
    public string? LogoUrl { get; set; }
}
