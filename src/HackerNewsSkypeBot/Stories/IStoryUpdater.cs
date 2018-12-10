using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Stories
{
    public interface IStoryUpdater
    {
        Task UpdateStoriesAsync();
    }
}
