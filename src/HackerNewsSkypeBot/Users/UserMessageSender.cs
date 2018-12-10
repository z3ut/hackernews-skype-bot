using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsSkypeBot.Users
{
    public class UserMessageSender : IUserMessageSender
    {
        public async Task SendAsync(User user, string message)
        {
            var userAccount = new ChannelAccount(user.Conversation.ToId, user.Conversation.ToName);
            var botAccount = new ChannelAccount(user.Conversation.FromId, user.Conversation.FromName);
            var connector = new ConnectorClient(new Uri(user.Conversation.ServiceUrl));

            string conversationId = user.Conversation.Id;

            IMessageActivity messageActiviry = Activity.CreateMessageActivity();
            if (!string.IsNullOrEmpty(user.Conversation.Id) &&
                !string.IsNullOrEmpty(user.Conversation.ChannelId))
            {
                // If conversation ID and channel ID was stored previously, use it.
                messageActiviry.ChannelId = user.Conversation.ChannelId;
            }
            else
            {
                // Conversation ID was not stored previously, so create a conversation.
                // Note: If the user has an existing conversation in a channel, this will likely create a new conversation window.
                conversationId = (await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount)).Id;
            }

            messageActiviry.From = botAccount;
            messageActiviry.Recipient = userAccount;
            messageActiviry.Conversation = new ConversationAccount(id: conversationId);
            messageActiviry.Text = message;
            messageActiviry.Locale = "en-us";

            await connector.Conversations.SendToConversationAsync((Activity)messageActiviry);
        }
    }
}
