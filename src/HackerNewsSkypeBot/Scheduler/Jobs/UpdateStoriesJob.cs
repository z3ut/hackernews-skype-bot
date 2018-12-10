using HackerNewsSkypeBot.Stories;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Scheduler.Jobs
{
    public class UpdateStoriesJob : IJob
    {
        private readonly IStoryUpdater _storyUpdater;

        public UpdateStoriesJob(IStoryUpdater storyUpdater)
        {
            _storyUpdater = storyUpdater;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _storyUpdater.UpdateStoriesAsync();
        }
    }
}
