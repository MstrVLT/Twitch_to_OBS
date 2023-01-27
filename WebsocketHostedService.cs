using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;
using Microsoft.Extensions.Logging;
using TwitchLib.EventSub.Websockets;
using TwitchLib.Api;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets.Handler.Channel.ChannelPoints.Redemptions;


namespace GenericHostConsoleApp
{
    internal class WebsocketHostedService : IHostedService
    {
        private readonly ILogger<WebsocketHostedService> _logger;
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

        private readonly ITwitchService _twitch;
        private readonly IOBSService _obs;
        public WebsocketHostedService(ILogger<WebsocketHostedService> logger, ITwitchService twitch, IOBSService obs, EventSubWebsocketClient eventSubWebsocketClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _obs = obs ?? throw new ArgumentNullException(nameof(obs));
            _twitch = twitch ?? throw new ArgumentNullException(nameof(twitch));

            _eventSubWebsocketClient = eventSubWebsocketClient ?? throw new ArgumentNullException(nameof(eventSubWebsocketClient));
            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnectedAsync;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;
            //channel.channel_points_custom_reward_redemption.add
            _eventSubWebsocketClient.ChannelPointsCustomRewardRedemptionAdd += OnChannelPointsCustomRewardRedemptionAdd;
        }
        private void OnChannelPointsCustomRewardRedemptionAdd(object? sender, ChannelPointsCustomRewardRedemptionArgs e)
        {
            var eventData = e.Notification.Payload.Event;
            if (eventData == null) return;

            _obs.RewardRedemption(eventData.Reward.Title);

            _logger.LogInformation($"{eventData.Reward.Title} {eventData.UserName} redeemed");
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.ConnectAsync();
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.DisconnectAsync();
        }
        private async void OnWebsocketConnectedAsync(object? sender, WebsocketConnectedArgs e)
        {
            _logger.LogInformation($"Websocket {_eventSubWebsocketClient.SessionId} connected!");

            if (e.IsRequestedReconnect) return;
            await _twitch.SubscribeToChannelRedemptionAddEvents(_eventSubWebsocketClient.SessionId);

            if (!_obs.Connect()) return;
        }
        private async void OnWebsocketDisconnected(object? sender, EventArgs e)
        {
            _logger.LogError($"Websocket {_eventSubWebsocketClient.SessionId} disconnected!");

            // Don't do this in production. You should implement a better reconnect strategy with exponential backoff
            while (!await _eventSubWebsocketClient.ReconnectAsync())
            {
                _logger.LogError("Websocket reconnect failed!");
                await Task.Delay(1000);
            }
        }
        private void OnWebsocketReconnected(object? sender, EventArgs e)
        {
            _logger.LogWarning($"Websocket {_eventSubWebsocketClient.SessionId} reconnected");
        }
        private void OnErrorOccurred(object? sender, ErrorOccuredArgs e)
        {
            _logger.LogError($"Websocket {_eventSubWebsocketClient.SessionId} - Error occurred!");
        }
    }
}
