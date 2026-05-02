using BabaPlay.Application.Queries.Roles;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Roles;

public class GetRolesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMappedRoleList()
    {
        var roleRepo = new Mock<IRoleRepository>();
        var tenantId = Guid.NewGuid();

        roleRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                Role.Create(tenantId, "Admin", null),
                Role.Create(tenantId, "Member", null)
            ]);

        var handler = new GetRolesQueryHandler(roleRepo.Object);
        var result = await handler.HandleAsync(new GetRolesQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Select(x => x.Name).Should().Contain(["Admin", "Member"]);
    }
}
