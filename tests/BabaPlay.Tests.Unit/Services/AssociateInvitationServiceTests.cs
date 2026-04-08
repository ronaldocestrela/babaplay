using BabaPlay.Modules.Associates.Entities;
using BabaPlay.Modules.Associates.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class AssociateInvitationServiceTests
{
    private readonly Mock<ITenantRepository<AssociateInvitation>> _invitations;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly AssociateInvitationService _sut;

    public AssociateInvitationServiceTests()
    {
        _invitations = new Mock<ITenantRepository<AssociateInvitation>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new AssociateInvitationService(_invitations.Object, _uow.Object);
    }

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_SingleUseWithoutEmail_ReturnsInvalid()
    {
        var result = await _sut.CreateAsync(null, isSingleUse: true, "user-1", TimeSpan.FromDays(1));

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Error.Should().Contain("Email");
    }

    [Fact]
    public async Task CreateAsync_EmptyInvitedByUserId_ReturnsUnauthorized()
    {
        var result = await _sut.CreateAsync("a@b.com", isSingleUse: true, "  ", TimeSpan.FromDays(1));

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task CreateAsync_ZeroTtl_ReturnsInvalid()
    {
        var result = await _sut.CreateAsync(null, isSingleUse: false, "user-1", TimeSpan.Zero);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task CreateAsync_DuplicatePendingSingleUse_ReturnsConflict()
    {
        var email = "dup@example.com";
        var now = DateTime.UtcNow;
        var pending = new AssociateInvitation
        {
            Email = email,
            IsSingleUse = true,
            Token = "existing-token",
            InvitedByUserId = "u1",
            ExpiresAt = now.AddDays(1)
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { pending }.AsAsyncQueryable());

        var result = await _sut.CreateAsync(email, isSingleUse: true, "user-2", TimeSpan.FromDays(7));

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task CreateAsync_SharedInvitation_Succeeds()
    {
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation>().AsAsyncQueryable());

        var result = await _sut.CreateAsync(null, isSingleUse: false, "user-1", TimeSpan.FromDays(7));

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();
        result.Value.Email.Should().BeNull();
        _invitations.Verify(r => r.AddAsync(It.IsAny<AssociateInvitation>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SingleUseWithEmail_Succeeds()
    {
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation>().AsAsyncQueryable());

        var result = await _sut.CreateAsync("new@example.com", isSingleUse: true, "user-1", TimeSpan.FromDays(7));

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task CreateAsync_SaveChangesThrowsDbUpdateException_ReturnsConflict()
    {
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation>().AsAsyncQueryable());
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("duplicate", (Exception?)null));

        var result = await _sut.CreateAsync(null, isSingleUse: false, "user-1", TimeSpan.FromDays(1));

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Error.Should().Contain("token");
    }

    // ── ValidateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_EmptyToken_ReturnsInvalid()
    {
        var result = await _sut.ValidateAsync("   ");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_TokenNotFound_ReturnsNotFound()
    {
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation>().AsAsyncQueryable());

        var result = await _sut.ValidateAsync("missing-token");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ValidateAsync_SingleUseAlreadyAccepted_ReturnsConflict()
    {
        var inv = new AssociateInvitation
        {
            Token = "t1",
            IsSingleUse = true,
            AcceptedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            InvitedByUserId = "u1"
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { inv }.AsAsyncQueryable());

        var result = await _sut.ValidateAsync("t1");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task ValidateAsync_Expired_ReturnsInvalid()
    {
        var inv = new AssociateInvitation
        {
            Token = "t1",
            IsSingleUse = false,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            InvitedByUserId = "u1"
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { inv }.AsAsyncQueryable());

        var result = await _sut.ValidateAsync("t1");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task ValidateAsync_Valid_ReturnsSuccess()
    {
        var inv = new AssociateInvitation
        {
            Token = "t1",
            IsSingleUse = false,
            Email = "e@x.com",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            InvitedByUserId = "u1"
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { inv }.AsAsyncQueryable());

        var result = await _sut.ValidateAsync("t1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("t1");
        result.Value.Email.Should().Be("e@x.com");
        result.Value.IsSingleUse.Should().BeFalse();
    }

    // ── MarkAcceptedAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAcceptedAsync_EmptyToken_ReturnsInvalid()
    {
        var result = await _sut.MarkAcceptedAsync(" ", "user-1");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task MarkAcceptedAsync_EmptyAcceptedByUserId_ReturnsInvalid()
    {
        var result = await _sut.MarkAcceptedAsync("t1", " ");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task MarkAcceptedAsync_NotFound_ReturnsNotFound()
    {
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation>().AsAsyncQueryable());

        var result = await _sut.MarkAcceptedAsync("missing", "user-1");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task MarkAcceptedAsync_SingleUseAlreadyAccepted_ReturnsConflict()
    {
        var inv = new AssociateInvitation
        {
            Token = "t1",
            IsSingleUse = true,
            AcceptedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            InvitedByUserId = "u1"
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { inv }.AsAsyncQueryable());

        var result = await _sut.MarkAcceptedAsync("t1", "user-2");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task MarkAcceptedAsync_Expired_ReturnsInvalid()
    {
        var inv = new AssociateInvitation
        {
            Token = "t1",
            IsSingleUse = false,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            InvitedByUserId = "u1"
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { inv }.AsAsyncQueryable());

        var result = await _sut.MarkAcceptedAsync("t1", "user-1");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task MarkAcceptedAsync_SingleUse_SetsAcceptedAndIncrementsUses()
    {
        AssociateInvitation? captured = null;
        var inv = new AssociateInvitation
        {
            Token = "t1",
            IsSingleUse = true,
            Email = "e@x.com",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            InvitedByUserId = "u1",
            UsesCount = 0
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { inv }.AsAsyncQueryable());
        _invitations.Setup(r => r.Update(It.IsAny<AssociateInvitation>()))
            .Callback<AssociateInvitation>(i => captured = i);

        var result = await _sut.MarkAcceptedAsync("t1", "new-user");

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.AcceptedAt.Should().NotBeNull();
        captured.AcceptedByUserId.Should().Be("new-user");
        captured.UsesCount.Should().Be(1);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAcceptedAsync_MultiUse_OnlyIncrementsUses()
    {
        AssociateInvitation? captured = null;
        var inv = new AssociateInvitation
        {
            Token = "t1",
            IsSingleUse = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            InvitedByUserId = "u1",
            UsesCount = 0
        };
        _invitations.Setup(r => r.Query()).Returns(new List<AssociateInvitation> { inv }.AsAsyncQueryable());
        _invitations.Setup(r => r.Update(It.IsAny<AssociateInvitation>()))
            .Callback<AssociateInvitation>(i => captured = i);

        var result = await _sut.MarkAcceptedAsync("t1", "new-user");

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.AcceptedAt.Should().BeNull();
        captured.AcceptedByUserId.Should().BeNull();
        captured.UsesCount.Should().Be(1);
    }
}
