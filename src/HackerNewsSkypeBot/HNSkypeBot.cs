using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HackerNewsSkypeBot.Storage;
using HackerNewsSkypeBot.Users;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace HackerNewsSkypeBot
{
    public class HNSkypeBot : IBot
    {
        private const string INTRO_MESSAGE = @"\
Hacker News bot.
Send notifications about stories with points above your subscription.
Usage:
Type 'sub MIN_POINTS' to receive notifications about stories with more than MIN_POINTS points.
Type 'unsub' to unsubscribe from notifications.
Type 'intro' or 'help' to see this message.
";

        private readonly UserDataAccessors _accessors;
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IUserUnreadStorySender _userUnreadStorySender;

        public HNSkypeBot(UserDataAccessors accessors, ILoggerFactory loggerFactory, IUserStorage userStorage, IUserUnreadStorySender userUnreadStorySender)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<HNSkypeBot>();
            _logger.LogTrace("EchoBot turn start.");
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            _userStorage = userStorage;
            _userUnreadStorySender = userUnreadStorySender;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var state = await _accessors.UserData.GetAsync(turnContext, () => new UserData());

                var text = turnContext.Activity.Text.ToLowerInvariant();
                var messageWords = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                switch (messageWords[0])
                {
                    case "intro":
                    case "help":
                        await turnContext.SendActivityAsync(INTRO_MESSAGE);
                        break;

                    case "sub":
                        if (!int.TryParse(messageWords[1], out int newsMinPoints))
                        {
                            await turnContext.SendActivityAsync($"sub command usage: 'sub MIN_POINTS', where MIN_POINTS - positive integer number.");
                            break;
                        }

                        User user;

                        if (state.UserId == null)
                        {
                            user = await _userStorage.GetUserAsync(turnContext.Activity.From.Id);
                            if (user != null)
                            {
                                state.UserId = user.Id;
                            }
                            else
                            {
                                var message = turnContext.Activity.AsMessageActivity();
                                var conversation = new Conversation()
                                {
                                    ToId = message.From.Id,
                                    ToName = message.From.Name,
                                    FromId = message.Recipient.Id,
                                    FromName = message.Recipient.Name,
                                    ServiceUrl = message.ServiceUrl,
                                    ChannelId = message.ChannelId,
                                    Id = message.Conversation.Id
                                };
                                user = new User(turnContext.Activity.From.Id, turnContext.Activity.From.Name, conversation);
                                await _userStorage.AddOrUpdateUserAsync(user);
                                state.UserId = user.Id;
                            }

                            await _accessors.UserData.SetAsync(turnContext, state);
                            await _accessors.ConversationState.SaveChangesAsync(turnContext);
                        }
                        else
                        {
                            user = await _userStorage.GetUserAsync(state.UserId);
                        }

                        user.IsSubscribedToStories = true;
                        user.StoriesMinPointsToNotify = newsMinPoints;

                        await _userStorage.AddOrUpdateUserAsync(user);
                        await turnContext.SendActivityAsync($"You have been subscribed to news with more than {newsMinPoints} points.");
                        await _userUnreadStorySender.SendToUserAsync(state.UserId);
                        break;

                    case "unsub":
                        if (state.UserId == null)
                        {
                            await turnContext.SendActivityAsync($"You are not subscribed to news.");
                            break;
                        }

                        var unsubUser = await _userStorage.GetUserAsync(state.UserId);
                        await _userStorage.AddOrUpdateUserAsync(unsubUser);
                        await turnContext.SendActivityAsync($"You have been successfully unsubscribed.");
                        break;

                    default:
                        await turnContext.SendActivityAsync("Sorry, can't understand you. Type \"help\" to list commands.");
                        break;
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded.Any())
                {
                    foreach (var member in turnContext.Activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message
                        // the 'bot' is the recipient for events from the channel,
                        // turnContext.Activity.MembersAdded == turnContext.Activity.Recipient.Id indicates the
                        // bot was added to the conversation.
                        if (member.Id != turnContext.Activity.Recipient.Id)
                        {
                            await turnContext.SendActivityAsync(INTRO_MESSAGE);
                        }
                    }
                }
            }
        }
    }
}
