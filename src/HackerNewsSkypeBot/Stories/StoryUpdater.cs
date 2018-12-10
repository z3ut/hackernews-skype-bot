using HackerNewsSkypeBot.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Stories
{
    public class StoryUpdater : IStoryUpdater
    {
        private IStoryStorage _storyStorage;
        private IHNStoryProvider _hNStoryProvider;

        public StoryUpdater(IStoryStorage storyStorage,
            IHNStoryProvider hNStoryProvider)
        {
            _storyStorage = storyStorage;
            _hNStoryProvider = hNStoryProvider;
        }

        public async Task UpdateStoriesAsync()
        {
            var topStories = await _hNStoryProvider.GetTopStoriesAsync();

            foreach (var s in topStories)
            {
                try
                {
                    var story = new Story(s.Id.ToString(), s.Title, s.Url, s.User, s.Points, s.Time);
                    await _storyStorage.AddOrUpdateStoryAsync(story);
                }
                catch (Exception) { }
            }
        }
    }
}
