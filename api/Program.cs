using System.Reflection;
using api.Data;
using api.Models;
using api.Publishers;
using api.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// cors
services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .Build());
});

// ioc
services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase(databaseName: "Test"));

services.AddScoped<DataSeeder>();
services.AddScoped<IClientRepository, ClientRepository>();
services.AddScoped<IClientEventPublisher, ClientEventPublisher>();

services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/clients", async (IClientRepository clientRepository) =>
{
    return await clientRepository.Get();
})
.WithName("get clients")
.Produces<Client[]>();

app.MapPost("/clients", async (IClientRepository clientRepository, IClientEventPublisher clientEventPublisher, 
        IValidator<Client> validator, Client client) =>
{
    var validationResult = validator.Validate(client);

    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());

    await clientRepository.Create(client);
    await clientEventPublisher.OnCreate(client);
    
    return Results.Created($"/clients/{client.Id}", client);
})
.WithName("create client")
.ProducesValidationProblem()
.Produces<Client>(201);

app.MapPut("/clients", async (IClientRepository clientRepository, 
        IValidator<Client> validator, [FromBody] Client client) =>
{
    var validationResult = validator.Validate(client);

    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());

    await clientRepository.Update(client);
    return Results.Ok(client);
})
.WithName("edit client")
.ProducesValidationProblem()
.Produces<Client>(200);

app.MapGet("/clients/search/{searchQuery}", 
        async (IClientRepository clientRepository, string searchQuery) =>
{
    if (string.IsNullOrWhiteSpace(searchQuery))
        return Array.Empty<Client>();

    return await clientRepository.Search(searchQuery);
})
.WithName("search clients")
.Produces<Client[]>();

app.MapGet("/clients/{id}", async (IClientRepository clientRepository, string id) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Provide a valid id.");

    var client = await clientRepository.Find(id);

    return client == null ? Results.NotFound() : Results.Ok(client);
})
.WithName("find client by id")
.Produces<Client>()
.ProducesProblem(400)
.ProducesProblem(404);

app.UseCors();

// seed data
using (var scope = app.Services.CreateScope())
{
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();

    dataSeeder.Seed();
}

// run app
app.Run();