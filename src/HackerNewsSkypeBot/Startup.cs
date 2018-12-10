using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HackerNewsSkypeBot.Scheduler;
using HackerNewsSkypeBot.Scheduler.Jobs;
using HackerNewsSkypeBot.Storage;
using HackerNewsSkypeBot.Storage.CosmosDB;
using HackerNewsSkypeBot.Storage.Memory;
using HackerNewsSkypeBot.Stories;
using HackerNewsSkypeBot.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HackerNewsSkypeBot
{
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private bool _isProduction = false;
        private IUserStoryScheduler _userStoryScheduler;
        private IContainer _container;

        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            services.AddHttpClient();

            var endpointUrl = Configuration.GetValue<string>("cosmosDB:endpointUrl");
            var primaryKey = Configuration.GetValue<string>("cosmosDB:primaryKey");
            var databaseName = Configuration.GetValue<string>("cosmosDB:databaseName");

            IStorage dataStore;
            bool isProduction = true;

#if DEBUG
            isProduction = false;
#endif

            if (isProduction)
            {
                var cosmosDbOptions = new CosmosDbStorageOptions();
                cosmosDbOptions.CosmosDBEndpoint = new Uri(endpointUrl);
                cosmosDbOptions.AuthKey = primaryKey;
                cosmosDbOptions.DatabaseId = databaseName;
                cosmosDbOptions.CollectionId = databaseName;
                dataStore = new CosmosDbStorage(cosmosDbOptions);

                builder.RegisterType<CosmosDBStoryStorage>()
                    .As<IStoryStorage>()
                    .WithParameter("endpointUrl", endpointUrl)
                    .WithParameter("primaryKey", primaryKey)
                    .WithParameter("databaseName", databaseName)
                    .InstancePerLifetimeScope();

                builder.RegisterType<CosmosDBUserStorage>()
                    .As<IUserStorage>()
                    .WithParameter("endpointUrl", endpointUrl)
                    .WithParameter("primaryKey", primaryKey)
                    .WithParameter("databaseName", databaseName)
                    .InstancePerLifetimeScope();

                builder.RegisterType<CosmosDBUserStoryNotificationStorage>()
                    .As<IUserStoryNotificationStorage>()
                    .WithParameter("endpointUrl", endpointUrl)
                    .WithParameter("primaryKey", primaryKey)
                    .WithParameter("databaseName", databaseName)
                    .InstancePerLifetimeScope();
            }
            else
            {
                dataStore = new MemoryStorage();

                builder.RegisterType<MemoryStoryStorage>()
                    .As<IStoryStorage>()
                    .InstancePerLifetimeScope();

                builder.RegisterType<MemoryUserStorage>()
                    .As<IUserStorage>()
                    .InstancePerLifetimeScope();

                builder.RegisterType<MemoryUserStoryNotificationStorage>()
                    .As<IUserStoryNotificationStorage>()
                    .InstancePerLifetimeScope();
            }

            var hNTopStoriesUrl = Configuration.GetValue<string>("hNTopStoriesUrl");

            builder.RegisterType<HNStoryProvider>()
                .As<IHNStoryProvider>()
                .WithParameter("hNTopStoriesUrl", hNTopStoriesUrl)
                .InstancePerLifetimeScope();

            builder.RegisterType<UserUnreadStorySender>()
                .As<IUserUnreadStorySender>()
                .InstancePerLifetimeScope();

            builder.RegisterType<StoryUpdater>()
                .As<IStoryUpdater>()
                .InstancePerLifetimeScope();

            builder.RegisterType<UserMessageSender>()
                .As<IUserMessageSender>()
                .InstancePerLifetimeScope();

            var cleanStorageCron = Configuration.GetValue<string>("scheduler:timers:cleanStorageCron");
            var sendUnreadStoriesToUsersCron = Configuration.GetValue<string>("scheduler:timers:sendUnreadStoriesToUsersCron");
            var updateStoriesCron = Configuration.GetValue<string>("scheduler:timers:updateStoriesCron");

            builder.RegisterType<UserStoryScheduler>()
                .As<IUserStoryScheduler>()
                .WithParameter("cleanStorageCron", cleanStorageCron)
                .WithParameter("sendUnreadStoriesToUsersCron", sendUnreadStoriesToUsersCron)
                .WithParameter("updateStoriesCron", updateStoriesCron)
                .InstancePerLifetimeScope();

            builder.RegisterType<CleanStorageJob>();
            builder.RegisterType<SendUnreadStoriesToUsersJob>();
            builder.RegisterType<UpdateStoriesJob>();

            services.AddBot<HNSkypeBot>(options =>
            {
                var secretKey = Configuration.GetSection("botFileSecret")?.Value;
                var botFilePath = Configuration.GetSection("botFilePath")?.Value;

                var botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
                services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded. ({botConfig})"));

                var environment = _isProduction ? "production" : "development";
                var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == environment).FirstOrDefault();
                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
                }

                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                ILogger logger = _loggerFactory.CreateLogger<HNSkypeBot>();
                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };

                var conversationState = new ConversationState(dataStore);

                options.State.Add(conversationState);
            });

            services.AddSingleton<UserDataAccessors>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the state accessors");
                }

                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                if (conversationState == null)
                {
                    throw new InvalidOperationException("ConversationState must be defined and added before adding conversation-scoped state accessors.");
                }

                var accessors = new UserDataAccessors(conversationState)
                {
                    UserData = conversationState.CreateProperty<UserData>(UserDataAccessors.UserDataName),
                };

                return accessors;
            });

            builder.Populate(services);
            _container = builder.Build();

            return new AutofacServiceProvider(_container);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();

            _userStoryScheduler = _container.Resolve<IUserStoryScheduler>();
            _userStoryScheduler.StartAsync().Wait();
        }
    }
}
