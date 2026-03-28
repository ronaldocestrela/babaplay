using BabaPlay.Modules.Financial.Entities;
using BabaPlay.Modules.Financial.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class CashEntryServiceTests
{
    private readonly Mock<ITenantRepository<CashEntry>> _entryRepo;
    private readonly Mock<ITenantRepository<Category>> _categoryRepo;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly CashEntryService _sut;

    public CashEntryServiceTests()
    {
        _entryRepo = new Mock<ITenantRepository<CashEntry>>();
        _categoryRepo = new Mock<ITenantRepository<Category>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new CashEntryService(_entryRepo.Object, _categoryRepo.Object, _uow.Object);
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_ReturnsEntriesOrderedByEntryDateDescending()
    {
        var older = new CashEntry { Amount = 50m, CategoryId = "c1", EntryDate = DateTime.UtcNow.AddDays(-1) };
        var newer = new CashEntry { Amount = 100m, CategoryId = "c1", EntryDate = DateTime.UtcNow };

        _entryRepo.Setup(r => r.Query()).Returns(new List<CashEntry> { older, newer }.AsAsyncQueryable());

        var result = await _sut.ListAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Amount.Should().Be(100m);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_CategoryNotFound_ReturnsNotFound()
    {
        _categoryRepo.Setup(r => r.GetByIdAsync("c-missing", It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Category?)null);

        var result = await _sut.CreateAsync(100m, "c-missing", null, null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
        _entryRepo.Verify(r => r.AddAsync(It.IsAny<CashEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidData_UsesProvidedEntryDate()
    {
        var entries = new List<CashEntry>();
        var category = new Category { Id = "c1", Name = "Mensalidade", Type = CategoryType.Income };
        _categoryRepo.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _entryRepo.Setup(r => r.AddAsync(It.IsAny<CashEntry>(), It.IsAny<CancellationToken>()))
                  .Callback<CashEntry, CancellationToken>((entry, _) =>
                  {
                      entry.Category = category;
                      entries.Add(entry);
                  })
                  .Returns(Task.CompletedTask);
        _entryRepo.Setup(r => r.Update(It.IsAny<CashEntry>()));
        _entryRepo.Setup(r => r.Query()).Returns(() => entries.AsAsyncQueryable());

        var date = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var result = await _sut.CreateAsync(250m, "c1", "Pagamento mensal", date, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(250m);
        result.Value.CurrentBalance.Should().Be(250m);
        result.Value.EntryDate.Should().Be(date);
        result.Value.Description.Should().Be("Pagamento mensal");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Create_NoEntryDate_UsesUtcNow()
    {
        var entries = new List<CashEntry>();
        var category = new Category { Id = "c1", Type = CategoryType.Income };
        _categoryRepo.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _entryRepo.Setup(r => r.AddAsync(It.IsAny<CashEntry>(), It.IsAny<CancellationToken>()))
                  .Callback<CashEntry, CancellationToken>((entry, _) =>
                  {
                      entry.Category = category;
                      entries.Add(entry);
                  })
                  .Returns(Task.CompletedTask);
        _entryRepo.Setup(r => r.Update(It.IsAny<CashEntry>()));
        _entryRepo.Setup(r => r.Query()).Returns(() => entries.AsAsyncQueryable());

        var before = DateTime.UtcNow;
        var result = await _sut.CreateAsync(100m, "c1", null, null, CancellationToken.None);
        var after = DateTime.UtcNow;

        result.IsSuccess.Should().BeTrue();
        result.Value.EntryDate.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        result.Value.CurrentBalance.Should().Be(100m);
    }

    [Fact]
    public async Task Create_ExpenseType_WithNegativeAmount_SubtractsAbsoluteValueFromBalance()
    {
        var existing = new CashEntry
        {
            Amount = 500m,
            CurrentBalance = 500m,
            Category = new Category { Id = "income", Type = CategoryType.Income },
            CategoryId = "income",
            EntryDate = DateTime.UtcNow.AddDays(-1)
        };
        var entries = new List<CashEntry> { existing };

        var expenseCategory = new Category { Id = "expense", Type = CategoryType.Expense };
        _categoryRepo.Setup(r => r.GetByIdAsync("expense", It.IsAny<CancellationToken>())).ReturnsAsync(expenseCategory);
        _entryRepo.Setup(r => r.AddAsync(It.IsAny<CashEntry>(), It.IsAny<CancellationToken>()))
            .Callback<CashEntry, CancellationToken>((entry, _) =>
            {
                entry.Category = expenseCategory;
                entries.Add(entry);
            })
            .Returns(Task.CompletedTask);
        _entryRepo.Setup(r => r.Update(It.IsAny<CashEntry>()));
        _entryRepo.Setup(r => r.Query()).Returns(() => entries.AsAsyncQueryable());

        var result = await _sut.CreateAsync(-120m, "expense", "Despesa", DateTime.UtcNow, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CurrentBalance.Should().Be(380m);
    }
}
