using HackerNewsSkypeBot.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Storage
{
    public interface IUserStorage
    {
        Task AddOrUpdateUserAsync(User user);
        Task<User> GetUserAsync(string userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetAllSubscribedUsersAsync();
    }
}
