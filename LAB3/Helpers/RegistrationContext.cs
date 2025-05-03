using LAB3.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAB3.Helpers
{
    public class CosmosSettings
    {
        public string EndpointUrl { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
    }

    public class RegistrationContext
    {
        protected Database Database { get; set; }
        protected DocumentCollection Collection { get; set; }
        protected DocumentClient Client { get; set; }
        protected CosmosSettings CosmosSettings { get; set; }

        public RegistrationContext(IOptions<CosmosSettings> cosmosSettings)
        {
            CosmosSettings = cosmosSettings.Value;
            Client = new DocumentClient(new Uri(CosmosSettings.EndpointUrl), CosmosSettings.AuthorizationKey);
        }

        public async Task ConfigureConnectionAsync()
        {
            Database = await Client.CreateDatabaseIfNotExistsAsync(new Database { Id = CosmosSettings.DatabaseId });

            Collection = await Client.CreateDocumentCollectionIfNotExistsAsync(
                Database.SelfLink,
                new DocumentCollection { Id = CosmosSettings.CollectionId });
        }

        public async Task<string> SaveEventRegistrationAsync(EventRegistration registration)
        {
            ResourceResponse<Document> response = await Client.CreateDocumentAsync(Collection.SelfLink, registration);
            return response.Resource.Id;
        }

        public async Task<List<EventRegistration>> GetRegistrationsForEventAsync(string eventKey)
        {
            var query = Client.CreateDocumentQuery<EventRegistration>(Collection.SelfLink)
                              .Where(r => r.EventKey == eventKey)
                              .AsDocumentQuery();

            List<EventRegistration> registrations = new List<EventRegistration>();
            while (query.HasMoreResults)
            {
                registrations.AddRange(await query.ExecuteNextAsync<EventRegistration>());
            }
            return registrations;
        }
    }
}
