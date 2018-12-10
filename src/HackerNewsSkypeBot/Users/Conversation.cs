using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Users
{
    public class Conversation
    {
        public string ToId { get; set; }
        public string ToName { get; set; }
        public string FromId { get; set; }
        public string FromName { get; set; }
        public string ServiceUrl { get; set; }
        public string ChannelId { get; set; }
        public string Id { get; set; }
    }
}
