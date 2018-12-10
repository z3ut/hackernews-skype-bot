using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Users
{
    public interface IUserMessageSender
    {
        Task SendAsync(User user, string message);
    }
}
