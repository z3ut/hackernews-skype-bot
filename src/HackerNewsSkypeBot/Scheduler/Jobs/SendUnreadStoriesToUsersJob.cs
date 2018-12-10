using HackerNewsSkypeBot.Users;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Scheduler.Jobs
{
    public class SendUnreadStoriesToUsersJob : IJob
    {
        private readonly IUserUnreadStorySender _userUnreadStorySender;

        public SendUnreadStoriesToUsersJob(IUserUnreadStorySender userUnreadStorySender)
        {
            _userUnreadStorySender = userUnreadStorySender;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _userUnreadStorySender.SendToAllUsersAsync();
        }
    }
}
