using HackerNewsSkypeBot.Storage;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Scheduler.Jobs
{
    public class CleanStorageJob : IJob
    {
        private readonly IStoryStorage _storyStorage;
        private readonly IUserStorage _userStorage;
        private readonly IUserStoryNotificationStorage _userStoryNotificationStorage;

        public CleanStorageJob(IStoryStorage storyStorage, IUserStorage userStorage,
            IUserStoryNotificationStorage userStoryNotificationStorage)
        {
            _storyStorage = storyStorage;
            _userStorage = userStorage;
            _userStoryNotificationStorage = userStoryNotificationStorage;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cleanBeforeDate = DateTime.Now.AddDays(-7);

            await _storyStorage.RemoveStoriesNotUpdatedSinceAsync(cleanBeforeDate);
            await _userStoryNotificationStorage.RemoveNotificationsBeforeDateAsync(cleanBeforeDate);
        }
    }
}
