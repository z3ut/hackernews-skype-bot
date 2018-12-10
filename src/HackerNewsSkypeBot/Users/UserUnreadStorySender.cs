using HackerNewsSkypeBot.Storage;
using HackerNewsSkypeBot.Stories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Users
{
    public class UserUnreadStorySender : IUserUnreadStorySender
    {
        private readonly IStoryStorage _storyStorage;
        private readonly IUserStorage _userStorage;
        private readonly IUserStoryNotificationStorage _userStoryNotificationStorage;
        private readonly IUserMessageSender _userMessageSender;

        public UserUnreadStorySender(
            IStoryStorage storyStorage,
            IUserStorage userStorage,
            IUserStoryNotificationStorage userStoryNotificationStorage,
            IUserMessageSender userMessageSender)
        {
            _storyStorage = storyStorage;
            _userStorage = userStorage;
            _userStoryNotificationStorage = userStoryNotificationStorage;
            _userMessageSender = userMessageSender;
        }

        public async Task SendToAllUsersAsync()
        {
            var subscribedUsers = await _userStorage.GetAllSubscribedUsersAsync();

            var currentStories = await _storyStorage.GetAllStoriesAsync();

            foreach (var user in subscribedUsers)
            {
                await SendUnnotifiedStoriesToUserAsync(user, currentStories);
            }
        }

        public async Task SendToUserAsync(string userId)
        {
            var user = await _userStorage.GetUserAsync(userId);

            if (user == null || !user.IsSubscribedToStories)
            {
                return;
            }

            var storiesAboveUserSubscription = await _storyStorage
                .GetAllStoriesAbovePointsAsync(user.StoriesMinPointsToNotify);

            await SendUnnotifiedStoriesToUserAsync(user, storiesAboveUserSubscription);
        }

        private async Task SendUnnotifiedStoriesToUserAsync(User user,
            IEnumerable<Story> currentStories)
        {
            var userNotifications = await _userStoryNotificationStorage
                .GetUserNotificationsAsync(user.Id);

            var unnotifiedStories = currentStories
                .Where(s => s.Points > user.StoriesMinPointsToNotify)
                .Where(s => !userNotifications.Any(un => un.StoryId == s.Id));

            foreach (var story in unnotifiedStories)
            {
                try
                {
                    var message = $"{story.Id}\n{story.Title}\n{story.Url}";
                    await _userMessageSender.SendAsync(user, message);
                    await _userStoryNotificationStorage
                        .SaveUserNotificationAsync(user.Id, story.Id);
                }
                catch (Exception) { }
            }
        }
    }
}
