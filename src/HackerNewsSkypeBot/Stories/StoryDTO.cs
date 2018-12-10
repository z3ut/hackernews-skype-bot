using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Stories
{
    public class StoryDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string User { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Points { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Time { get; set; }
    }
}
