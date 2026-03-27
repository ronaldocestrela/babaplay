using BabaPlay.Modules.Financial.Entities;
using BabaPlay.Modules.Financial.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class CategoryServiceTests
{
    private readonly Mock<ITenantRepository<Category>> _repo;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _repo = new Mock<ITenantRepository<Category>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new CategoryService(_repo.Object, _uow.Object);
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_ReturnsCategoriesOrderedByName()
    {
        var categories = new List<Category>
        {
            new() { Name = "Mensalidade" },
            new() { Name = "Evento" }
        };
        _repo.Setup(r => r.Query()).Returns(categories.AsAsyncQueryable());

        var result = await _sut.ListAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Evento");
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyName_ReturnsInvalid(string name)
    {
        var result = await _sut.CreateAsync(name, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        _repo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidName_PersistsAndReturnsCategory()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync("  Mensalidade  ", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Mensalidade");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
