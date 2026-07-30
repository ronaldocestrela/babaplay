using System;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BabaPlay.Tests.Unit.Domain;

public class AnnouncementDomainTests
{
    private static readonly Guid ValidTenantId = Guid.NewGuid();
    private static readonly Guid ValidAuthorId = Guid.NewGuid();
    private const string ValidTitle = "Informativo: Mudança de Horário";
    private const string ValidContent = "O horário do baba de sábado passa a ser às 9h a partir desta semana.";

    [Fact]
    public void Create_WithValidData_ShouldCreateUnpublishedAnnouncement()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent);

        announcement.Should().NotBeNull();
        announcement.TenantId.Should().Be(ValidTenantId);
        announcement.AuthorId.Should().Be(ValidAuthorId);
        announcement.Title.Should().Be(ValidTitle);
        announcement.Content.Should().Be(ValidContent);
        announcement.IsPublished.Should().BeFalse();
        announcement.PublishedAtUtc.Should().BeNull();
        announcement.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        var act = () => Announcement.Create(Guid.Empty, ValidAuthorId, ValidTitle, ValidContent);

        act.Should().Throw<ArgumentException>().WithMessage("*TenantId*");
    }

    [Fact]
    public void Create_WithEmptyAuthorId_ShouldThrowArgumentException()
    {
        var act = () => Announcement.Create(ValidTenantId, Guid.Empty, ValidTitle, ValidContent);

        act.Should().Throw<ArgumentException>().WithMessage("*AuthorId*");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        var act = () => Announcement.Create(ValidTenantId, ValidAuthorId, string.Empty, ValidContent);

        act.Should().Throw<ArgumentException>().WithMessage("*Title*");
    }

    [Fact]
    public void Create_WithWhitespaceContent_ShouldThrowArgumentException()
    {
        var act = () => Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, "   ");

        act.Should().Throw<ArgumentException>().WithMessage("*Content*");
    }

    [Fact]
    public void Publish_WhenDraft_ShouldSetIsPublishedAndTimestamp()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent);
        var before = DateTime.UtcNow;

        announcement.Publish();

        announcement.IsPublished.Should().BeTrue();
        announcement.PublishedAtUtc.Should().NotBeNull();
        announcement.PublishedAtUtc!.Value.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldBeIdempotent()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent);
        announcement.Publish();
        var firstPublishedAt = announcement.PublishedAtUtc;

        announcement.Publish(); // call again

        announcement.IsPublished.Should().BeTrue();
        announcement.PublishedAtUtc.Should().Be(firstPublishedAt); // unchanged
    }

    [Fact]
    public void Unpublish_WhenPublished_ShouldSetIsPublishedFalse()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent);
        announcement.Publish();

        announcement.Unpublish();

        announcement.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveAndIsPublishedFalse()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent);
        announcement.Publish();

        announcement.Deactivate();

        announcement.IsActive.Should().BeFalse();
        announcement.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void SetExpiry_ShouldStoreUtcDate()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent);
        var expiry = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        announcement.SetExpiry(expiry);

        announcement.ExpiresAtUtc.Should().Be(expiry);
    }

    [Fact]
    public void Create_WithOptionalTags_ShouldStoreTrimmedTags()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent, tags: " importante, horário ");

        announcement.Tags.Should().Be("importante, horário");
    }

    [Fact]
    public void Create_WithNullTags_ShouldStoreNull()
    {
        var announcement = Announcement.Create(ValidTenantId, ValidAuthorId, ValidTitle, ValidContent, tags: null);

        announcement.Tags.Should().BeNull();
    }
}
