using HackerNewsSkypeBot.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Storage.Memory
{
    public class MemoryUserStoryNotificationStorage : IUserStoryNotificationStorage
    {
        private static List<UserStoryNotification> _notifications;

        static MemoryUserStoryNotificationStorage()
        {
            _notifications = new List<UserStoryNotification>();
        }

        public async Task<IEnumerable<UserStoryNotification>> GetUserNotificationsAsync(string userId)
        {
            return _notifications.Where(n => n.UserId == userId);
        }

        public async Task RemoveNotificationsBeforeDateAsync(DateTime datetime)
        {
            _notifications = _notifications
                .Where(n => n.NotificationDatetime > datetime)
                .ToList();
        }

        public async Task SaveUserNotificationAsync(string userId, string storyId)
        {
            var notification = new UserStoryNotification(userId, storyId, DateTime.Now);
            _notifications.Add(notification);
        }
    }
}
