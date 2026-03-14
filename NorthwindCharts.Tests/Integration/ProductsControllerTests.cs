using System.Net;
using NorthwindCharts.Tests.Infrastructure;

namespace NorthwindCharts.Tests.Integration;

public sealed class ProductsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Products_Index_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Products", html);
    }

    [Fact]
    public async Task Categories_Delete_WithReferences_RedirectsWithError()
    {
        var response = await _client.PostAsync("/Categories/Delete/1", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Categories_Delete_WithoutReferences_RedirectsSuccess()
    {
        var response = await _client.PostAsync("/Categories/Delete/2", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
