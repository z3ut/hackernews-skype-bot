using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerNewsSkypeBot.Users;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HackerNewsSkypeBot.Storage.CosmosDB
{
    public class CosmosDBUserStorage : IUserStorage
    {
        private static string USER_COLLECTION_NAME = "User";

        private readonly string _endpointUrl;
        private readonly string _primaryKey;
        private readonly string _databaseName;

        public CosmosDBUserStorage(string endpointUrl,
            string primaryKey, string databaseName)
        {
            _endpointUrl = endpointUrl;
            _primaryKey = primaryKey;
            _databaseName = databaseName;
        }

        private DocumentClient _client;
        private DocumentClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new DocumentClient(new Uri(_endpointUrl), _primaryKey);

                    _client.CreateDatabaseIfNotExistsAsync(
                            new Database { Id = _databaseName })
                        .Wait();

                    _client.CreateDocumentCollectionIfNotExistsAsync(
                            UriFactory.CreateDatabaseUri(_databaseName),
                            new DocumentCollection { Id = USER_COLLECTION_NAME })
                        .Wait();
                }

                return _client;
            }
        }

        public async Task AddOrUpdateUserAsync(Users.User user)
        {
            await Client.UpsertDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, USER_COLLECTION_NAME),
                    user);
        }

        public async Task<IEnumerable<Users.User>> GetAllSubscribedUsersAsync()
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var stories = Client.CreateDocumentQuery<Users.User>(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, USER_COLLECTION_NAME),
                    queryOptions)
                .Where(u => u.IsSubscribedToStories == true);

            return stories;
        }

        public async Task<IEnumerable<Users.User>> GetAllUsersAsync()
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var users = Client.CreateDocumentQuery<Users.User>(
                UriFactory.CreateDocumentCollectionUri(
                    _databaseName, USER_COLLECTION_NAME),
                queryOptions);

            return users;
        }

        public async Task<Users.User> GetUserAsync(string userId)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var user = Client.CreateDocumentQuery<Users.User>(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, USER_COLLECTION_NAME),
                    queryOptions)
                .Where(u => u.Id == userId)
                .Take(1)
                .AsEnumerable()
                .FirstOrDefault();

            return user;
        }
    }
}
