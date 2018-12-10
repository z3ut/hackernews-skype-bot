using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Stories
{
    public class Story
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string User { get; set; }
        public int Points { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdate { get; set; }

        public Story(string id, string title, string url,
            string user, int points, DateTime created)
        {
            Id = id;
            Title = title;
            Url = url;
            User = user;
            Points = points;
            Created = created;
        }
    }
}
