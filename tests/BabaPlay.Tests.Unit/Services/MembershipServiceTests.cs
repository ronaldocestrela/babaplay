using BabaPlay.Modules.Financial.Entities;
using BabaPlay.Modules.Financial.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class MembershipServiceTests
{
    private readonly Mock<ITenantRepository<Membership>> _membershipRepo;
    private readonly Mock<ITenantRepository<Payment>> _paymentRepo;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly MembershipService _sut;

    public MembershipServiceTests()
    {
        _membershipRepo = new Mock<ITenantRepository<Membership>>();
        _paymentRepo = new Mock<ITenantRepository<Payment>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new MembershipService(_membershipRepo.Object, _paymentRepo.Object, _uow.Object);
    }

    // ── ListForAssociate ──────────────────────────────────────────────────────

    [Fact]
    public async Task ListForAssociate_ReturnsMembershipsOrderedByYearMonthDesc()
    {
        var data = new List<Membership>
        {
            new() { AssociateId = "a1", Year = 2025, Month = 3 },
            new() { AssociateId = "a1", Year = 2025, Month = 1 },
            new() { AssociateId = "a2", Year = 2025, Month = 5 } // different associate
        };
        _membershipRepo.Setup(r => r.Query()).Returns(data.AsAsyncQueryable());

        var result = await _sut.ListForAssociateAsync("a1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Month.Should().Be(3);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1999, 1)]
    [InlineData(2000, 0)]
    [InlineData(2000, 13)]
    public async Task Create_InvalidPeriod_ReturnsInvalid(int year, int month)
    {
        var result = await _sut.CreateAsync("a1", year, month, 100m, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Error.Should().Contain("Invalid period");
    }

    [Fact]
    public async Task Create_DuplicatePeriod_ReturnsConflict()
    {
        var existing = new List<Membership>
        {
            new() { AssociateId = "a1", Year = 2025, Month = 3 }
        };
        _membershipRepo.Setup(r => r.Query()).Returns(existing.AsAsyncQueryable());

        var result = await _sut.CreateAsync("a1", 2025, 3, 100m, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Create_ValidData_PersistsAndReturnsMembership()
    {
        _membershipRepo.Setup(r => r.Query()).Returns(new List<Membership>().AsAsyncQueryable());
        _membershipRepo.Setup(r => r.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync("a1", 2025, 4, 150m, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssociateId.Should().Be("a1");
        result.Value.Year.Should().Be(2025);
        result.Value.Month.Should().Be(4);
        result.Value.Amount.Should().Be(150m);
        result.Value.Status.Should().Be(MembershipStatus.Pending);
    }

    // ── RegisterPayment ───────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterPayment_MembershipNotFound_ReturnsNotFound()
    {
        _membershipRepo.Setup(r => r.GetByIdAsync("m99", It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Membership?)null);

        var result = await _sut.RegisterPaymentAsync("m99", 100m, "pix", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task RegisterPayment_ValidData_CreatesPaymentAndMarksMembershipPaid()
    {
        var membership = new Membership { Id = "m1", AssociateId = "a1", Year = 2025, Month = 4, Amount = 150m };
        _membershipRepo.Setup(r => r.GetByIdAsync("m1", It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        _paymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var result = await _sut.RegisterPaymentAsync("m1", 150m, "pix", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MembershipId.Should().Be("m1");
        result.Value.Amount.Should().Be(150m);
        result.Value.Method.Should().Be("pix");
        membership.Status.Should().Be(MembershipStatus.Paid);
        _membershipRepo.Verify(r => r.Update(membership), Times.Once);
    }
}
