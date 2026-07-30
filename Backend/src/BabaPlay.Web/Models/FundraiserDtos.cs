using System;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

public sealed record FundraiserDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public decimal CollectedAmount { get; init; }
    public double ProgressPercentage { get; init; }
    public DateTime? DeadlineUtc { get; init; }
    public int Status { get; init; } // 0 = Active, 1 = Completed, 2 = Cancelled
    public int ContributionsCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CreateFundraiserDto
{
    [Required(ErrorMessage = "O título da arrecadação é obrigatório.")]
    [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres.")]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "A meta de arrecadação (R$) é obrigatória.")]
    [Range(0.01, 100000.00, ErrorMessage = "A meta deve ser maior que R$ 0,00.")]
    public decimal TargetAmount { get; set; } = 500m;

    public DateTime? DeadlineUtc { get; set; }
}

public sealed class ContributeFundraiserDto
{
    [Required(ErrorMessage = "O valor da contribuição é obrigatório.")]
    [Range(0.01, 10000.00, ErrorMessage = "O valor deve ser maior que R$ 0,00.")]
    public decimal Amount { get; set; } = 50m;

    public Guid? PlayerId { get; set; }

    public string? ContributorName { get; set; }
}
