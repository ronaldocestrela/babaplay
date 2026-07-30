using System;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

public sealed record InvoiceDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid PlayerId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal OpenAmount { get; init; }
    public DateTime DueDateUtc { get; init; }
    public DateTime? PaidAtUtc { get; init; }
    public int Status { get; init; } // 0: Open, 1: Paid, 2: Overdue, 4: Cancelled
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CreateInvoiceDto
{
    [Required(ErrorMessage = "Selecione o atleta.")]
    public Guid PlayerId { get; set; }

    [Required(ErrorMessage = "O ano é obrigatório.")]
    [Range(2000, 2100, ErrorMessage = "Ano inválido.")]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [Required(ErrorMessage = "O mês é obrigatório.")]
    [Range(1, 12, ErrorMessage = "Mês inválido.")]
    public int Month { get; set; } = DateTime.UtcNow.Month;

    [Required(ErrorMessage = "Informe o valor.")]
    [Range(0.01, 100000.00, ErrorMessage = "Valor deve ser maior que zero.")]
    public decimal Amount { get; set; } = 50.00m;

    [Required(ErrorMessage = "Informe a data de vencimento.")]
    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow.Date.AddDays(7);

    public string? Notes { get; set; }
}
