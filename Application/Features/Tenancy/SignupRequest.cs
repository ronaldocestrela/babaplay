namespace Application.Features.Tenancy;

using Application.Features.Associados;

public class SignupRequest
{
    // Tenant information
    public string? Identifier { get; set; }
    public required string AssociationName { get; set; }

    // Association contact/address (kept in tenant metadata)
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string PhoneNumber { get; set; }

    // Admin (lead) information — reuse CreateAssociadoRequest
    public required CreateAssociadoRequest Admin { get; set; }
}
