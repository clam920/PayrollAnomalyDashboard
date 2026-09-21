using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace Payroll.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("payroll_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString()
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Starts an actual Postgres 16 container (Docker pulls the image on
        // first run). Program.cs's own dbContext.Database.Migrate() call
        // then applies the real EF Core migrations against it during host
        // startup - there's no separate migration step to write here.
        await _postgresContainer.StartAsync();
    }

    // Since endpoints now require a Bearer token (JWT auth), every test
    // needs an authenticated HttpClient rather than the bare CreateClient().
    // Centralized here so test classes don't each duplicate the login round
    // trip and header-setting boilerplate.
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var client = CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
        loginResponse.EnsureSuccessStatusCode();

        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);

        return client;
    }

    private record LoginResponse(string Token, string Username, string Role);

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await base.DisposeAsync();
    }
}