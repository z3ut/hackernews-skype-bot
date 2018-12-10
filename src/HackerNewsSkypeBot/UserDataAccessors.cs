using System;
using Microsoft.Bot.Builder;

namespace HackerNewsSkypeBot
{
    public class UserDataAccessors
    {
        public UserDataAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string UserDataName { get; } = $"{nameof(UserDataAccessors)}.UserData";

        public IStatePropertyAccessor<UserData> UserData { get; set; }

        public ConversationState ConversationState { get; }
    }
}
