using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure;
using BabaPlay.Infrastructure.Services;
using BabaPlay.Infrastructure.Workers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BabaPlay.Tests.Unit.Infrastructure;

public class ServiceRegistrationEmailTests
{
    [Fact]
    public void AddInfrastructureServices_ShouldRegisterEmailServicesAndWorker()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MasterDb"] = "Server=localhost;Database=BabaPlay_Master;User Id=sa;Password=Test@123;TrustServerCertificate=true;",
                ["Jwt:SecretKey"] = "test-secret-key-at-least-256-bits-long-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                ["Jwt:Issuer"] = "BabaPlay.Api",
                ["Jwt:Audience"] = "BabaPlay.Clients",
                ["ResendEmail:FromEmail"] = "noreply@babaplay.com",
                ["ResendEmail:FromName"] = "BabaPlay",
            })
            .Build();

        services.AddInfrastructureServices(configuration);

        services.Should().Contain(d => d.ServiceType == typeof(IEmailService));
        services.Should().Contain(d => d.ServiceType == typeof(IEmailDispatchQueue) && d.ImplementationType == typeof(EmailDispatchQueue));
        services.Should().Contain(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(EmailDispatchWorker));
    }
}
