using System.Net;
using System.Text.Json;
using NorthwindCharts.Tests.Infrastructure;

namespace NorthwindCharts.Tests.Integration;

public sealed class DashboardApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DashboardApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TopProducts_ReturnsExpectedJsonShape()
    {
        var response = await _client.GetAsync("/dashboard/api/top-products?topN=5");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.GetArrayLength() > 0);
        Assert.True(doc.RootElement[0].TryGetProperty("productName", out _));
        Assert.True(doc.RootElement[0].TryGetProperty("totalQuantity", out _));
    }

    [Fact]
    public async Task MonthlySales_ReturnsExpectedJsonShape()
    {
        var response = await _client.GetAsync("/dashboard/api/monthly-sales?year=1997");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement[0].TryGetProperty("salesMonth", out _));
        Assert.True(doc.RootElement[0].TryGetProperty("totalSales", out _));
    }
}
