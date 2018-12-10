using HackerNewsSkypeBot.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Storage
{
    public interface IUserStoryNotificationStorage
    {
        Task SaveUserNotificationAsync(string userId, string storyId);
        Task<IEnumerable<UserStoryNotification>> GetUserNotificationsAsync(string userId);
        Task RemoveNotificationsBeforeDateAsync(DateTime datetime);
    }
}
