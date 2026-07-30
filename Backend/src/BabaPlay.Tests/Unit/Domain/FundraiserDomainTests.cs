using System;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BabaPlay.Tests.Unit.Domain;

public class FundraiserDomainTests
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateActiveFundraiser()
    {
        var tenantId = Guid.NewGuid();
        var fundraiser = Fundraiser.Create(tenantId, "Churrasco Fim de Ano", "Vaquinha para compra das carnes", 1000m);

        fundraiser.Should().NotBeNull();
        fundraiser.TenantId.Should().Be(tenantId);
        fundraiser.Title.Should().Be("Churrasco Fim de Ano");
        fundraiser.TargetAmount.Should().Be(1000m);
        fundraiser.CollectedAmount.Should().Be(0m);
        fundraiser.Status.Should().Be(FundraiserStatus.Active);
        fundraiser.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AddContribution_ShouldIncreaseCollectedAmountAndIncrementCount()
    {
        var tenantId = Guid.NewGuid();
        var fundraiser = Fundraiser.Create(tenantId, "Bolas Novas", "Vaquinha para comprar 5 bolas", 500m);

        fundraiser.AddContribution(200m);

        fundraiser.CollectedAmount.Should().Be(200m);
        fundraiser.ContributionsCount.Should().Be(1);
        fundraiser.Status.Should().Be(FundraiserStatus.Active);
    }

    [Fact]
    public void AddContribution_WhenReachingTarget_ShouldCompleteFundraiser()
    {
        var tenantId = Guid.NewGuid();
        var fundraiser = Fundraiser.Create(tenantId, "Bolas Novas", "Vaquinha para comprar 5 bolas", 500m);

        fundraiser.AddContribution(500m);

        fundraiser.CollectedAmount.Should().Be(500m);
        fundraiser.Status.Should().Be(FundraiserStatus.Completed);
    }
}
