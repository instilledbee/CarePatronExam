using System.Net;
using System.Net.Http.Json;
using api.Data;
using api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests;

public class ClientApiIntegrationTests
{
    [SetUp]
    public async Task SetUp()
    {
        var app = new ClientApiApplication();

        // ensure the test DB is cleared per test
        using var scope = app.Services.CreateScope();
        
        var dataContext = scope.ServiceProvider.GetService<DataContext>();
        var allClients = dataContext.Clients.ToList();
        dataContext.Clients.RemoveRange(allClients);
        await dataContext.SaveChangesAsync();
    }
    
    [Test]
    public async Task Create_ReturnsSuccess_IfModelIsValid()
    {
        var app = new ClientApiApplication();
        var client = app.CreateClient();

        var model = new Client()
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Junvic",
            LastName = "Valdez",
            Email = "contact@junvic.me",
            PhoneNumber = "+123456789"
        };

        var createResponse = await client.PostAsJsonAsync("/clients", model);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var found = await client.GetFromJsonAsync<Client>(createResponse.Headers.Location);
        Assert.That(found.Id, Is.EqualTo(model.Id));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_IfModelIsInvalid()
    {
        var app = new ClientApiApplication();
        var client = app.CreateClient();

        var model = new Client()
        {
        };

        var createResponse = await client.PostAsJsonAsync("/clients", model);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetClients_ReturnsAllItems()
    {
        var app = new ClientApiApplication();
        var client = app.CreateClient();

        var testItems = new List<Client>()
        {
            new Client()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Junvic",
                LastName = "Valdez",
                Email = "contact@junvic.me",
                PhoneNumber = "+123456789"
            },
            new Client()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                PhoneNumber = "+183561908"
            }
        };

        foreach (var item in testItems)
            await client.PostAsJsonAsync("/clients", item);

        var results = await client.GetFromJsonAsync<Client[]>("/clients");
        
        Assert.That(results.Length, Is.EqualTo(testItems.Count));
    }

    [Test]
    public async Task Update_SetsCorrectDetails_IfModelIsValid()
    {
        var app = new ClientApiApplication();
        var client = app.CreateClient();

        var model = new Client()
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Junvic",
            LastName = "Valdez",
            Email = "contact@junvic.me",
            PhoneNumber = "+123456789"
        };

        await client.PostAsJsonAsync("/clients", model);

        model.FirstName = "John";
        model.PhoneNumber = "+1987654321";

        var response = await client.PutAsJsonAsync("/clients", model);
        var responseModel = await response.Content.ReadFromJsonAsync<Client>();
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseModel.FirstName, Is.EqualTo("John"));
        Assert.That(responseModel.LastName, Is.EqualTo("Valdez"));
        Assert.That(responseModel.Email, Is.EqualTo("contact@junvic.me"));
        Assert.That(responseModel.PhoneNumber, Is.EqualTo("+1987654321"));

    }
    
    [Test]
    public async Task Update_ReturnsBadRequest_IfModelIsInvalid()
    {
        var app = new ClientApiApplication();
        var client = app.CreateClient();

        var model = new Client()
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Junvic",
            LastName = "Valdez",
            Email = "contact@junvic.me",
            PhoneNumber = "+123456789"
        };

        await client.PostAsJsonAsync("/clients", model);

        var response = await client.PutAsJsonAsync("/clients", new Client()
        {
            Id = model.Id
        });
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Search_ReturnsCorrectResults()
    {
        var app = new ClientApiApplication();
        var client = app.CreateClient();

        var testItems = new List<Client>()
        {
            new Client()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Junvic",
                LastName = "Valdez",
                Email = "contact@junvic.me",
                PhoneNumber = "+123456789"
            },
            new Client()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                PhoneNumber = "+183561908"
            }
        };

        foreach (var item in testItems)
            await client.PostAsJsonAsync("/clients", item);

        var results = await client.GetFromJsonAsync<Client[]>("/clients/search/john");
        Assert.That(results.Length, Is.EqualTo(1));
        
        results = await client.GetFromJsonAsync<Client[]>("/clients/search/j");
        Assert.That(results.Length, Is.EqualTo(2));
            
        results = await client.GetFromJsonAsync<Client[]>("/clients/search/Andy");
        Assert.That(results.Length, Is.EqualTo(0));
    }
}