using HackerNewsSkypeBot.Stories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Storage
{
    public interface IStoryStorage
    {
        Task AddOrUpdateStoryAsync(Story story);
        Task<Story> GetStoryAsync(string id);
        Task<IEnumerable<Story>> GetAllStoriesAsync();
        Task<IEnumerable<Story>> GetAllStoriesAbovePointsAsync(int points);
        Task RemoveStoriesNotUpdatedSinceAsync(DateTime datetime);
    }
}
