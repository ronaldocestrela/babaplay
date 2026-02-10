namespace Application.Features.Associados;

public class AssociadoResponse
{
    public required string Id { get; set; }
    public required string FullName { get; set; }
    public required string CPF { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string Position { get; set; }
    public required string Email { get; set; }
    public required string UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
