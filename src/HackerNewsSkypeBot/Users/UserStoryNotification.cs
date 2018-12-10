using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Users
{
    public class UserStoryNotification
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string StoryId { get; set; }
        public DateTime NotificationDatetime { get; set; }

        public UserStoryNotification(string userId, string storyId,
            DateTime notificationDatetime)
        {
            UserId = userId;
            StoryId = storyId;
            NotificationDatetime = notificationDatetime;
        }
    }
}
