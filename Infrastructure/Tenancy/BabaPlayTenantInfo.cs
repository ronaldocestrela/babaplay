using Finbuckle.MultiTenant.Abstractions;

namespace Infrastructure.Tenancy;

public class BabaPlayTenantInfo : ITenantInfo
{
    public required string Id { get; set; }
    public required string Identifier { get; set; }
    public required string Name { get; set; }
    public string? ConnectionString { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime ValidUpTo { get; set; }
    public bool IsActive { get; set; }
}
