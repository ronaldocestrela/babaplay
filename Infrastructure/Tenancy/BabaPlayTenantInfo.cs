using Finbuckle.MultiTenant.Abstractions;

namespace Infrastructure.Tenancy;

public class BabaPlayTenantInfo : ITenantInfo
{
    public string? Id { get; set; }
    public string? Identifier { get; set; }
    public string? Name { get; set; }
    public string? ConnectionString { get; set; }

    // Tenant owner / admin metadata
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Association contact/address stored in tenant metadata
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNumber { get; set; }

    public DateTime ValidUpTo { get; set; }
    public bool IsActive { get; set; }
}
