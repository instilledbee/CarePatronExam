using api.Models;

namespace api.Publishers;

public interface IClientEventPublisher
{
    Task OnCreate(Client client);
}

public class ClientEvent
{
    public ClientEventTypes EventType { get; set; }
    public Client Model { get; set; }
}

public enum ClientEventTypes
{
    Created,
    Updated
}

public class ClientEventPublisher : IClientEventPublisher
{
    private readonly ILogger<ClientEventPublisher> _logger;
    private static List<ClientEvent> _events = new();
    
    public ClientEventPublisher(ILogger<ClientEventPublisher> logger)
    {
        _logger = logger;
    }

    public async Task OnCreate(Client client)
    {
        // simulate sending to an external message service
        await Task.Run(() =>
        {
            _events.Add(new ClientEvent()
            {
                EventType = ClientEventTypes.Created,
                Model = client
            });
        });
        
        _logger.LogInformation($"Client created: {client.Id}");
        _logger.LogDebug($"Events in queue: {_events.Count}");
    }
}