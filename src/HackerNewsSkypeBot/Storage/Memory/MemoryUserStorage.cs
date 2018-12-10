using HackerNewsSkypeBot.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Storage.Memory
{
    public class MemoryUserStorage : IUserStorage
    {
        private static List<User> _users;

        static MemoryUserStorage()
        {
            _users = new List<User>();
        }

        public async Task AddOrUpdateUserAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                _users.Remove(existingUser);
            }

            _users.Add(user);
        }

        public async Task<User> GetUserAsync(string userId)
        {
            return _users.Find(u => u.Id == userId);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return _users;
        }

        public async Task<IEnumerable<User>> GetAllSubscribedUsersAsync()
        {
            return _users.Where(u => u.IsSubscribedToStories);
        }
    }
}
