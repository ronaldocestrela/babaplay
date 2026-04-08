using BabaPlay.SharedKernel.Web;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Web;

public sealed class TenantSlugResolverTests
{
    [Fact]
    public void Resolve_HeaderWinsOverQueryAndHost()
    {
        var slug = TenantSlugResolver.Resolve("from-header", "from-query", "club.example.com");

        slug.Should().Be("from-header");
    }

    [Fact]
    public void Resolve_QueryUsedWhenNoHeader_FlatDomainScenario()
    {
        var slug = TenantSlugResolver.Resolve(null, "refactest", "app.babaplay.com.br");

        slug.Should().Be("refactest");
    }

    [Fact]
    public void Resolve_HostSubdomainWhenNoHeaderOrQuery()
    {
        var slug = TenantSlugResolver.Resolve(null, null, "club.example.com");

        slug.Should().Be("club");
    }

    [Fact]
    public void Resolve_NoMatchOnSingleLabelHost()
    {
        var slug = TenantSlugResolver.Resolve(null, null, "localhost");

        slug.Should().BeNull();
    }
}
