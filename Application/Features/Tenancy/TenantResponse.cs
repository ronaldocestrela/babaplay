namespace Application.Features.Tenancy;

public class TenantResponse
{
    public required string Identifier { get; set; }
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime ValidUpTo { get; set; }
    public bool IsActive { get; set; }
}
