using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Users
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsSubscribedToStories { get; set; }
        public int StoriesMinPointsToNotify { get; set; }
        public Conversation Conversation { get; set; }

        public User(string id, string name, Conversation conversation)
        {
            Id = id;
            Name = name;
            Conversation = conversation;
        }
    }
}
