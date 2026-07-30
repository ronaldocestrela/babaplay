using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Reports;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Reports;

public class AttendanceWidgetTests : TestContext
{
    [Fact]
    public void AttendanceWidget_ShouldRenderAttendanceTableAndPercentages()
    {
        // Arrange
        var attendance = new List<AttendanceEntryDto>
        {
            new(Guid.NewGuid(), "Zico", "Galinho", null, 10, 10, 100.0),
            new(Guid.NewGuid(), "Pelé", "Rei", null, 9, 10, 90.0)
        };

        // Act
        var cut = RenderComponent<AttendanceWidget>(parameters => parameters
            .Add(p => p.AttendanceList, attendance));

        // Assert
        cut.Find("[data-testid='attendance-title']").TextContent.Should().Contain("Ranking de Assiduidade & Frequência");
        cut.Find("[data-testid='attendance-table']").Should().NotBeNull();
        cut.Markup.Should().Contain("100%");
        cut.Markup.Should().Contain("90%");
    }
}
