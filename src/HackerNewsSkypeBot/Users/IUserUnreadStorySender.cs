using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Users
{
    public interface IUserUnreadStorySender
    {
        Task SendToAllUsersAsync();
        Task SendToUserAsync(string userId);
    }
}
