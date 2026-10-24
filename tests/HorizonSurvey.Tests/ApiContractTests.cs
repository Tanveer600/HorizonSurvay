using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HorizonSurvey.Domain.Models;
using HorizonSurvey.Infrastructure.Persistence;

namespace HorizonSurvey.Tests;

public sealed class ApiContractTests
{
    [Fact]
    public async Task ApiStartsWithoutConnectingToOrCreatingTheSqlServerDatabase()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CategoryBackedEquipmentRequestsAndCheckoutErrorsUseHttpStatusCodes()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var missingCategory = await client.PostAsync("/api/equipment", Json("{\"name\":\"Unclassified asset\"}"));
        Assert.Equal(HttpStatusCode.BadRequest, missingCategory.StatusCode);
        var missingCategoryId = await client.PostAsync(
            "/api/equipment",
            Json($$"""{"name":"Unclassified asset","categoryId":"{{Guid.NewGuid()}}"}"""));
        Assert.Equal(HttpStatusCode.BadRequest, missingCategoryId.StatusCode);

        var categoryResponse = await client.PostAsync("/api/categories", Json("""{"name":"Drone"}"""));
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        using var categoryJson = JsonDocument.Parse(await categoryResponse.Content.ReadAsStringAsync());
        var droneCategoryId = categoryJson.RootElement.GetProperty("id").GetGuid();

        var droneResponse = await client.PostAsync(
            "/api/equipment",
            Json($$"""{"name":"Drone 1","categoryId":"{{droneCategoryId}}"}"""));
        Assert.Equal(HttpStatusCode.Created, droneResponse.StatusCode);
        using var droneJson = JsonDocument.Parse(await droneResponse.Content.ReadAsStringAsync());
        var droneId = droneJson.RootElement.GetProperty("id").GetGuid();

        var unknownAsset = await client.PostAsync(
            "/api/checkouts",
            Json(
                $$"""{"holder":"Amina","expectedReturnAt":"{{DateTimeOffset.UtcNow.AddDays(1):O}}","equipmentAssetId":"{{Guid.NewGuid()}}","quantity":1}"""));
        Assert.Equal(HttpStatusCode.BadRequest, unknownAsset.StatusCode);
        Assert.Contains("Invalid request", await unknownAsset.Content.ReadAsStringAsync());

        var missingApproval = await client.PostAsync(
            "/api/checkouts",
            Json($$"""{"holder":"Amina","expectedReturnAt":"{{DateTimeOffset.UtcNow.AddDays(1):O}}","equipmentAssetId":"{{droneId}}","quantity":1}"""));
        Assert.Equal(HttpStatusCode.Conflict, missingApproval.StatusCode);
        var error = await missingApproval.Content.ReadAsStringAsync();
        Assert.Contains("Inventory conflict", error);
    }

    [Fact]
    public async Task CategoryCrudAndMovementDeleteEndpointsExposeSafeResults()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        var categoryResponse = await client.PostAsync("/api/categories", Json("""{"name":"Rover"}"""));
        using var categoryJson = JsonDocument.Parse(await categoryResponse.Content.ReadAsStringAsync());
        var categoryId = categoryJson.RootElement.GetProperty("id").GetGuid();
        var duplicate = await client.PostAsync("/api/categories", Json("""{"name":"rover"}"""));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var equipmentResponse = await client.PostAsync("/api/equipment",
            Json($$"""{"name":"Rover 1","categoryId":"{{categoryId}}"}"""));
        Assert.Equal(HttpStatusCode.Created, equipmentResponse.StatusCode);
        var rename = await client.PutAsync($"/api/categories/{categoryId}", Json("""{"name":"Survey gear"}"""));
        Assert.Equal(HttpStatusCode.OK, rename.StatusCode);
        var inventoryAfterRename = await client.GetAsync("/api/inventory");
        using (var renamedInventoryJson = JsonDocument.Parse(await inventoryAfterRename.Content.ReadAsStringAsync()))
            Assert.Equal("Survey gear", Assert.Single(renamedInventoryJson.RootElement.GetProperty("equipment").EnumerateArray())
                .GetProperty("category").GetString());

        var checkoutResponse = await client.PostAsync("/api/checkouts",
            Json($$"""{"holder":"Amina","expectedReturnAt":"{{DateTimeOffset.UtcNow.AddDays(1):O}}","pooledStockId":"{{factory.PooledStockId}}","quantity":1}"""));
        Assert.Equal(HttpStatusCode.Created, checkoutResponse.StatusCode);
        var history = await client.GetAsync($"/api/history?from={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O"))}&to={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(1).ToString("O"))}");
        using var historyJson = JsonDocument.Parse(await history.Content.ReadAsStringAsync());
        var movementId = Assert.Single(historyJson.RootElement.EnumerateArray()).GetProperty("id").GetGuid();

        var deleted = await client.DeleteAsync($"/api/history/{movementId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        var openCheckouts = await client.GetAsync("/api/checkouts");
        using var openCheckoutsJson = JsonDocument.Parse(await openCheckouts.Content.ReadAsStringAsync());
        Assert.Single(openCheckoutsJson.RootElement.EnumerateArray());
        var rangeDeleteMissingBounds = await client.DeleteAsync("/api/history");
        Assert.Equal(HttpStatusCode.BadRequest, rangeDeleteMissingBounds.StatusCode);

        var secondCheckout = await client.PostAsync("/api/checkouts",
            Json($$"""{"holder":"Saima","expectedReturnAt":"{{DateTimeOffset.UtcNow.AddDays(1):O}}","pooledStockId":"{{factory.PooledStockId}}","quantity":1}"""));
        Assert.Equal(HttpStatusCode.Created, secondCheckout.StatusCode);
        var remainingMovementResponse = await client.GetAsync(
            $"/api/history?from={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O"))}&to={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(1).ToString("O"))}");
        using var remainingMovementsJson = JsonDocument.Parse(await remainingMovementResponse.Content.ReadAsStringAsync());
        var remainingMovement = Assert.Single(remainingMovementsJson.RootElement.EnumerateArray());
        var occurredAt = remainingMovement.GetProperty("occurredAt").GetString();
        var rangeDelete = await client.DeleteAsync(
            $"/api/history?from={Uri.EscapeDataString(occurredAt!)}&to={Uri.EscapeDataString(occurredAt!)}");
        Assert.Equal(HttpStatusCode.OK, rangeDelete.StatusCode);
        using var rangeResult = JsonDocument.Parse(await rangeDelete.Content.ReadAsStringAsync());
        Assert.Equal(1, rangeResult.RootElement.GetProperty("deletedCount").GetInt32());
        var historyAfterRangeDelete = await client.GetAsync(
            $"/api/history?from={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O"))}&to={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(1).ToString("O"))}");
        using var emptyHistoryJson = JsonDocument.Parse(await historyAfterRangeDelete.Content.ReadAsStringAsync());
        Assert.Empty(emptyHistoryJson.RootElement.EnumerateArray());
        var inventoryAfterMovementDelete = await client.GetAsync("/api/inventory");
        using var inventoryAfterMovementDeleteJson = JsonDocument.Parse(await inventoryAfterMovementDelete.Content.ReadAsStringAsync());
        Assert.Equal(2, Assert.Single(inventoryAfterMovementDeleteJson.RootElement.GetProperty("pooledStock").EnumerateArray())
            .GetProperty("availableQuantity").GetInt32());
        var checkoutsAfterRangeDelete = await client.GetAsync("/api/checkouts");
        using var checkoutsAfterRangeDeleteJson = JsonDocument.Parse(await checkoutsAfterRangeDelete.Content.ReadAsStringAsync());
        Assert.Equal(2, checkoutsAfterRangeDeleteJson.RootElement.GetArrayLength());

        var categoryDelete = await client.DeleteAsync($"/api/categories/{categoryId}");
        Assert.Equal(HttpStatusCode.NoContent, categoryDelete.StatusCode);
        var inventoryResponse = await client.GetAsync("/api/inventory");
        using var inventoryJson = JsonDocument.Parse(await inventoryResponse.Content.ReadAsStringAsync());
        var asset = Assert.Single(inventoryJson.RootElement.GetProperty("equipment").EnumerateArray());
        Assert.Null(asset.GetProperty("categoryId").GetString());
    }

    [Fact]
    public async Task InventoryEndpointReturnsExplicitlyPersistedPooledStock()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/inventory");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var inventoryJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var stock = Assert.Single(inventoryJson.RootElement.GetProperty("pooledStock").EnumerateArray());
        Assert.Equal(factory.PooledStockId, stock.GetProperty("id").GetGuid());
        Assert.Equal("Test tripods", stock.GetProperty("name").GetString());
        Assert.Equal(4, stock.GetProperty("totalQuantity").GetInt32());
        Assert.Equal(4, stock.GetProperty("availableQuantity").GetInt32());
    }

    [Fact]
    public async Task OpenCheckoutsRemainReadableWithIdentityAndOutstandingQuantityAfterReload()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        var checkoutResponse = await client.PostAsync(
            "/api/checkouts",
            Json($$"""{"holder":"Amina","expectedReturnAt":"{{DateTimeOffset.UtcNow.AddDays(1):O}}","pooledStockId":"{{factory.PooledStockId}}","quantity":3}"""));
        Assert.Equal(HttpStatusCode.Created, checkoutResponse.StatusCode);
        using var checkoutJson = JsonDocument.Parse(await checkoutResponse.Content.ReadAsStringAsync());
        var checkoutId = checkoutJson.RootElement.GetProperty("id").GetGuid();

        using var reloadedClient = factory.CreateClient();
        var listResponse = await reloadedClient.GetAsync("/api/checkouts");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listJson = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var openCheckout = Assert.Single(listJson.RootElement.EnumerateArray());
        Assert.Equal(checkoutId, openCheckout.GetProperty("id").GetGuid());
        Assert.Equal("Test tripods", openCheckout.GetProperty("itemName").GetString());
        Assert.Equal("Amina", openCheckout.GetProperty("holder").GetString());
        Assert.Equal(3, openCheckout.GetProperty("outstandingQuantity").GetInt32());
        Assert.False(openCheckout.GetProperty("isOverdue").GetBoolean());
        Assert.Equal(
            factory.PooledStockId,
            openCheckout.GetProperty("pooledStockId").GetGuid());

        var returnResponse = await reloadedClient.PostAsync(
            $"/api/checkouts/{checkoutId}/returns",
            Json("""{"returner":"Saima","quantity":1}"""));
        Assert.Equal(HttpStatusCode.OK, returnResponse.StatusCode);

        var afterPartialReturn = await reloadedClient.GetAsync("/api/checkouts");
        using var afterPartialReturnJson = JsonDocument.Parse(await afterPartialReturn.Content.ReadAsStringAsync());
        Assert.Equal(2, Assert.Single(afterPartialReturnJson.RootElement.EnumerateArray())
            .GetProperty("outstandingQuantity").GetInt32());

        var finalReturn = await reloadedClient.PostAsync(
            $"/api/checkouts/{checkoutId}/returns",
            Json("""{"returner":"Saima","quantity":2}"""));
        Assert.Equal(HttpStatusCode.OK, finalReturn.StatusCode);
        var afterClosure = await reloadedClient.GetAsync("/api/checkouts");
        using var afterClosureJson = JsonDocument.Parse(await afterClosure.Content.ReadAsStringAsync());
        Assert.Empty(afterClosureJson.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task CorsAllowsOnlyConfiguredFrontendOrigin()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        using var allowedRequest = new HttpRequestMessage(HttpMethod.Options, "/api/checkouts");
        allowedRequest.Headers.Add("Origin", "http://localhost:5173");
        allowedRequest.Headers.Add("Access-Control-Request-Method", "GET");
        var allowedResponse = await client.SendAsync(allowedRequest);
        Assert.True(
            allowedResponse.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowedOrigins),
            $"Expected a CORS header but received {allowedResponse.StatusCode}: {string.Join("; ", allowedResponse.Headers.Select(header => $"{header.Key}={string.Join(",", header.Value)}"))}");
        Assert.Equal("http://localhost:5173", Assert.Single(allowedOrigins!));

        using var rejectedRequest = new HttpRequestMessage(HttpMethod.Options, "/api/checkouts");
        rejectedRequest.Headers.Add("Origin", "https://other.example");
        rejectedRequest.Headers.Add("Access-Control-Request-Method", "GET");
        var rejectedResponse = await client.SendAsync(rejectedRequest);
        Assert.False(rejectedResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static StringContent Json(string body) =>
        new(body, Encoding.UTF8, "application/json");

    private sealed class ApiTestFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        public Guid PooledStockId { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<InventoryDbContext>>();
                services.RemoveAll<InventoryDbContext>();
                services.AddDbContext<InventoryDbContext>(options => options.UseSqlite(_connection));
            });
            using var testContext = new InventoryDbContext(
                new DbContextOptionsBuilder<InventoryDbContext>()
                    .UseSqlite(_connection)
                    .Options);
            testContext.Database.EnsureCreated();
            var stock = new PooledStock("Test tripods", 4);
            testContext.PooledStocks.Add(stock);
            testContext.SaveChanges();
            PooledStockId = stock.Id;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _connection.Dispose();
        }
    }
}
