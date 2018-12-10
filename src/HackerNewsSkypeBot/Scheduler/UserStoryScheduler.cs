using HackerNewsSkypeBot.Scheduler.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Scheduler
{
    public class UserStoryScheduler : IUserStoryScheduler
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly string _cleanStorageCron;
        private readonly string _sendUnreadStoriesToUsersCron;
        private readonly string _updateStoriesCron;

        private IScheduler _scheduler;

        private IJobDetail _cleanStorageJobDetail;
        private IJobDetail _updateStoriesJobDetail;
        private IJobDetail _sendUnreadStoriesToUsersJobDetail;

        public UserStoryScheduler(IServiceProvider serviceProvider, string cleanStorageCron,
            string sendUnreadStoriesToUsersCron, string updateStoriesCron)
        {
            _serviceProvider = serviceProvider;

            _cleanStorageCron = cleanStorageCron;
            _sendUnreadStoriesToUsersCron = sendUnreadStoriesToUsersCron;
            _updateStoriesCron = updateStoriesCron;
        }

        public async Task StartAsync()
        {
            await ConfigureSchedulerAsync();
            await _scheduler.Start();
            await ConfigureJobsAsync();
            await TriggerJobsRequiredOnStartAsync();
        }

        public async Task StopAsync()
        {
            await _scheduler.Shutdown();
        }

        private async Task ConfigureSchedulerAsync()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory schedulerFactory = new StdSchedulerFactory(props);
            JobFactory jobFactory = new JobFactory(_serviceProvider);

            _scheduler = await schedulerFactory.GetScheduler();
            _scheduler.JobFactory = jobFactory;
        }

        private async Task ConfigureJobsAsync()
        {
            _updateStoriesJobDetail = JobBuilder.Create<UpdateStoriesJob>()
                .WithIdentity("UpdateStoriesJob", "group1")
                .Build();

            ITrigger updateStoriesTrigger = TriggerBuilder.Create()
                .WithIdentity("UpdateStoriesTrigger", "group1")
                .StartNow()
                .WithCronSchedule(_updateStoriesCron)
                .Build();

            await _scheduler.ScheduleJob(_updateStoriesJobDetail, updateStoriesTrigger);


            _sendUnreadStoriesToUsersJobDetail = JobBuilder.Create<SendUnreadStoriesToUsersJob>()
                .WithIdentity("SendUnreadStoriesToUsersJob", "group1")
                .Build();

            ITrigger userNotificationTrigger = TriggerBuilder.Create()
                .WithIdentity("SendUnreadStoriesToUsersTrigger", "group1")
                .StartNow()
                .WithCronSchedule(_sendUnreadStoriesToUsersCron)
                .Build();

            await _scheduler.ScheduleJob(_sendUnreadStoriesToUsersJobDetail, userNotificationTrigger);


            _cleanStorageJobDetail = JobBuilder.Create<CleanStorageJob>()
                .WithIdentity("CleanStorageJob", "group1")
                .Build();

            ITrigger cleadStorageTrigger = TriggerBuilder.Create()
                .WithIdentity("CleanStorageTrigger", "group1")
                .StartNow()
                .WithCronSchedule(_cleanStorageCron)
                .Build();

            await _scheduler.ScheduleJob(_cleanStorageJobDetail, cleadStorageTrigger);
        }

        private async Task TriggerJobsRequiredOnStartAsync()
        {
            await _scheduler.TriggerJob(_updateStoriesJobDetail.Key);
            await _scheduler.TriggerJob(_sendUnreadStoriesToUsersJobDetail.Key);
        }
    }
}
