using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public interface IClientRepository
    {
        Task<Client[]> Get();
        Task Create(Client client);
        Task Update(Client client);
        Task<Client[]> Search(string searchQuery);
    }

    public class ClientRepository : IClientRepository
    {
        private readonly DataContext dataContext;

        public ClientRepository(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task Create(Client client)
        {
            await dataContext.AddAsync(client);
            await dataContext.SaveChangesAsync();
        }

        public Task<Client[]> Get()
        {
            return dataContext.Clients.ToArrayAsync();
        }

        public async Task Update(Client client)
        {
            var existingClient = await dataContext.Clients.FirstOrDefaultAsync(x => x.Id == client.Id);

            if (existingClient == null)
                return;

            existingClient.FirstName = client.FirstName;
            existingClient.LastName = client.LastName;
            existingClient.Email = client.Email;
            existingClient.PhoneNumber = client.PhoneNumber;

            await dataContext.SaveChangesAsync();
        }

        public async Task<Client[]> Search(string searchQuery)
        {
            var matchingClients = dataContext.Clients.Where(x =>
                x.FirstName.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase) ||
                x.LastName.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase));

            return await matchingClients.ToArrayAsync();
        }
    }
}

