using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerNewsSkypeBot.Users;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HackerNewsSkypeBot.Storage.CosmosDB
{
    public class CosmosDBUserStoryNotificationStorage : IUserStoryNotificationStorage
    {
        private static string NOTIFICATION_COLLECTION_NAME = "Notification";

        private readonly string _endpointUrl;
        private readonly string _primaryKey;
        private readonly string _databaseName;

        public CosmosDBUserStoryNotificationStorage(string endpointUrl,
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
                            new DocumentCollection { Id = NOTIFICATION_COLLECTION_NAME })
                        .Wait();
                }

                return _client;
            }
        }

        public async Task<IEnumerable<UserStoryNotification>> GetUserNotificationsAsync(string userId)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var notifications = Client.CreateDocumentQuery<UserStoryNotification>(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, NOTIFICATION_COLLECTION_NAME),
                    queryOptions)
                .Where(n => n.UserId == userId);

            return notifications;
        }

        public async Task RemoveNotificationsBeforeDateAsync(DateTime datetime)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var notificationsToDelete = Client.CreateDocumentQuery<UserStoryNotification>(
                    UriFactory.CreateDocumentCollectionUri(
                        _databaseName, NOTIFICATION_COLLECTION_NAME),
                    queryOptions)
                .Where(n => n.NotificationDatetime < datetime);

            foreach (var notification in notificationsToDelete)
            {
                await Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(
                    _databaseName, NOTIFICATION_COLLECTION_NAME, notification.Id));
            }
        }

        public async Task SaveUserNotificationAsync(string userId, string storyId)
        {
            var notification = new UserStoryNotification(userId, storyId, DateTime.Now);
            notification.Id = Guid.NewGuid().ToString("N");

            await Client.CreateDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(
                    _databaseName, NOTIFICATION_COLLECTION_NAME),
                notification);
        }
    }
}
