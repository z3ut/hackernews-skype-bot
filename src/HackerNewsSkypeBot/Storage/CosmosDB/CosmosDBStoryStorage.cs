using HackerNewsSkypeBot.Stories;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Storage.CosmosDB
{
    public class CosmosDBStoryStorage : IStoryStorage
    {
        private static string STORY_COLLECTION_NAME = "Story";

        private readonly string _endpointUrl;
        private readonly string _primaryKey;
        private readonly string _databaseName;

        public CosmosDBStoryStorage(string endpointUrl,
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
                            new DocumentCollection { Id = STORY_COLLECTION_NAME })
                        .Wait();
                }

                return _client;
            }
        }

        public async Task AddOrUpdateStoryAsync(Story story)
        {
            await Client.UpsertDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(
                    _databaseName, STORY_COLLECTION_NAME),
                story);
        }

        public async Task<IEnumerable<Story>> GetAllStoriesAsync()
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var stories = Client.CreateDocumentQuery<Story>(
                UriFactory.CreateDocumentCollectionUri(
                    _databaseName, STORY_COLLECTION_NAME),
                queryOptions);

            return stories;
        }

        public async Task<IEnumerable<Story>> GetAllStoriesAbovePointsAsync(int points)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var stories = Client.CreateDocumentQuery<Story>(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, STORY_COLLECTION_NAME),
                    queryOptions)
                .Where(s => s.Points > points);

            return stories;
        }

        public async Task<Story> GetStoryAsync(string id)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var story = Client.CreateDocumentQuery<Story>(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, STORY_COLLECTION_NAME),
                    queryOptions)
                .Where(s => s.Id == id)
                .FirstOrDefault();

            return story;
        }

        public async Task RemoveStoriesNotUpdatedSinceAsync(DateTime datetime)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var storiesToDelete = Client.CreateDocumentQuery<Story>(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, STORY_COLLECTION_NAME),
                    queryOptions)
                .Where(s => s.LastUpdate < datetime);

            foreach (var story in storiesToDelete)
            {
                await Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(
                        _databaseName, STORY_COLLECTION_NAME, story.Id.ToString()));
            }
        }
    }
}
