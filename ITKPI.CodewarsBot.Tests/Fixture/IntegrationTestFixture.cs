﻿using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Xunit;
using ITKPI.CodewarsBot.Api.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ITKPI.CodewarsBot.Tests.Fixture
{
    public class IntegrationTestFixture : TestServer, IAsyncLifetime
    {
        public IntegrationTestFixture()
        : base(Configure())
        {
            
        }

        public static IWebHostBuilder Configure()
        {
            return new WebHostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appsettings.json", true, reloadOnChange: false);
                })
                .ConfigureServices(services =>
                {

                })
                .UseStartup<Startup>();
        }

        public T ResolveDependency<T>()
        {
            return Host.Services.GetRequiredService<T>();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var dbInfrastructure = ResolveDependency<DatabaseInfrastructure>();
            await dbInfrastructure.Drop();
        }
    }
}
