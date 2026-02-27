namespace Domain.Entities;

public class CorsOrigin : BaseEntity
{
    public required string Origin { get; set; }
    public bool IsActive { get; set; }
}
