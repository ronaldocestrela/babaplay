namespace Application.Features.Associados;

public class UpdateAssociadoRequest
{
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string Position { get; set; }
}
