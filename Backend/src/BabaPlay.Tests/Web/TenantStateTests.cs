using System;
using Xunit;
using FluentAssertions;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Tests.Web;

public class TenantStateTests
{
    [Fact]
    public void SetTenant_ShouldUpdateStateAndTriggerOnChangeEvent()
    {
        // Arrange
        var tenantState = new TenantState();
        var tenantId = Guid.NewGuid();
        var tenantName = "Baba dos Amigos";
        var eventTriggered = false;

        tenantState.OnChange += () => eventTriggered = true;

        // Act
        tenantState.SetTenant(tenantId, tenantName);

        // Assert
        tenantState.CurrentTenantId.Should().Be(tenantId);
        tenantState.CurrentTenantName.Should().Be(tenantName);
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void ClearTenant_ShouldResetStateAndTriggerOnChangeEvent()
    {
        // Arrange
        var tenantState = new TenantState();
        tenantState.SetTenant(Guid.NewGuid(), "Test Baba");
        var eventTriggered = false;

        tenantState.OnChange += () => eventTriggered = true;

        // Act
        tenantState.ClearTenant();

        // Assert
        tenantState.CurrentTenantId.Should().BeNull();
        tenantState.CurrentTenantName.Should().BeNull();
        eventTriggered.Should().BeTrue();
    }
}
