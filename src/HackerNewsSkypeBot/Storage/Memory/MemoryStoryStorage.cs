using HackerNewsSkypeBot.Stories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Storage.Memory
{
    public class MemoryStoryStorage : IStoryStorage
    {
        private static List<Story> _stories;

        static MemoryStoryStorage()
        {
            _stories = new List<Story>();
        }

        public async Task AddOrUpdateStoryAsync(Story story)
        {
            var existingStory = await GetStoryAsync(story.Id);

            if (existingStory != null)
            {
                existingStory.Points = story.Points;
                existingStory.LastUpdate = DateTime.Now;
            }
            else
            {
                story.LastUpdate = DateTime.Now;
                _stories.Add(story);
            }
        }

        public async Task<Story> GetStoryAsync(string id)
        {
            return _stories.Find(s => s.Id == id);
        }

        public async Task<IEnumerable<Story>> GetAllStoriesAsync()
        {
            return _stories;
        }

        public async Task<IEnumerable<Story>> GetAllStoriesAbovePointsAsync(int points)
        {
            return _stories.Where(s => s.Points > points);
        }

        public async Task RemoveStoriesNotUpdatedSinceAsync(DateTime datetime)
        {
            _stories = _stories
                .Where(s => s.LastUpdate > datetime)
                .ToList();
        }
    }
}
