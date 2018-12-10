using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Stories
{
    public class HNStoryProvider : IHNStoryProvider
    {
        private readonly string requestUrl;
        private readonly IHttpClientFactory _clientFactory;

        public HNStoryProvider(string hNTopStoriesUrl, IHttpClientFactory clientFactory)
        {
            requestUrl = hNTopStoriesUrl;
            _clientFactory = clientFactory;
        }

        public async Task<IEnumerable<StoryDTO>> GetTopStoriesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<IEnumerable<StoryDTO>>();
            }
            else
            {
                return new List<StoryDTO>();
            }
        }
    }
}
