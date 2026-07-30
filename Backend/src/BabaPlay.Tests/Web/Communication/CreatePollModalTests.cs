using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Communication;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Communication;

public class CreatePollModalTests : TestContext
{
    private readonly Mock<ICommunicationApiService> _mockService = new();

    public CreatePollModalTests()
    {
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public void CreatePollModal_WhenNotOpen_ShouldRenderNothing()
    {
        // Act
        var cut = RenderComponent<CreatePollModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        cut.FindAll("[data-testid='create-poll-modal']").Should().BeEmpty();
    }

    [Fact]
    public void CreatePollModal_WhenOpen_ShouldRenderFormWithInitialOptions()
    {
        // Act
        var cut = RenderComponent<CreatePollModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        cut.Find("[data-testid='create-poll-modal']").Should().NotBeNull();
        cut.Find("[data-testid='input-poll-title']").Should().NotBeNull();
        cut.FindAll("[data-testid='option-input-group']").Should().HaveCount(2);
    }

    [Fact]
    public async Task CreatePollModal_AddOptionButton_ShouldAddInputRow()
    {
        // Act
        var cut = RenderComponent<CreatePollModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        var addBtn = cut.Find("[data-testid='btn-add-option-input']");
        await addBtn.ClickAsync(new MouseEventArgs());

        // Assert
        cut.FindAll("[data-testid='option-input-group']").Should().HaveCount(3);
    }

    [Fact]
    public async Task CreatePollModal_SubmitValidForm_ShouldCallApiAndTriggerOnCreated()
    {
        // Arrange
        PollDto? createdDto = null;
        var mockResponse = new PollDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Admin",
            Title: "Título Criado",
            Description: "Descrição",
            ExpiresAtUtc: null,
            IsClosed: false,
            HasVoted: false,
            VotedOptionId: null,
            TotalVotes: 0,
            Options: [new(Guid.NewGuid(), "Opção 1", 1, 0, 0.0), new(Guid.NewGuid(), "Opção 2", 2, 0, 0.0)],
            CreatedAt: DateTime.UtcNow
        );

        _mockService
            .Setup(x => x.CreatePollAsync(It.IsAny<CreatePollDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var cut = RenderComponent<CreatePollModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnCreated, (PollDto dto) => createdDto = dto));

        // Fill title and options
        cut.Find("[data-testid='input-poll-title']").Change("Título Criado");
        cut.FindAll("[data-testid='input-option-text']")[0].Change("Opção A");
        cut.FindAll("[data-testid='input-option-text']")[1].Change("Opção B");

        // Submit form
        await cut.Find("[data-testid='form-create-poll']").SubmitAsync();

        // Assert
        _mockService.Verify(x => x.CreatePollAsync(It.IsAny<CreatePollDto>(), It.IsAny<CancellationToken>()), Times.Once);
        createdDto.Should().NotBeNull();
        createdDto!.Title.Should().Be("Título Criado");
    }
}
