using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Financial;

namespace BabaPlay.Tests.Web.Financial;

public class InvoiceStatusBadgeTests : TestContext
{
    [Theory]
    [InlineData(0, "Pendente", "badge-warning")]
    [InlineData(1, "Pago", "badge-success")]
    [InlineData(2, "Atrasado", "badge-danger")]
    [InlineData(4, "Cancelado", "badge-secondary")]
    public void InvoiceStatusBadge_ShouldRenderCorrectLabelAndCss(int status, string expectedLabel, string expectedCssClass)
    {
        // Act
        var cut = RenderComponent<InvoiceStatusBadge>(parameters => parameters
            .Add(p => p.Status, status));

        // Assert
        cut.Markup.Should().Contain(expectedLabel);
        cut.Markup.Should().Contain(expectedCssClass);
    }
}
