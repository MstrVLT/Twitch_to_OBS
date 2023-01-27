using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using static System.Formats.Asn1.AsnWriter;
using System.Xml.Linq;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.EventSub.Websockets;

namespace GenericHostConsoleApp;

internal sealed class TwitchService : ITwitchService
{
    private readonly ILogger<TwitchService> _logger;

    private readonly IOptions<TwitchSettings> _twitchSettings;

    private readonly TwitchAPI _twitchApi = new TwitchAPI();

    public TwitchService(
        ILogger<TwitchService> logger,
        IOptions<TwitchSettings> twitchSettings)
    {
        _logger = logger;
        _twitchSettings = twitchSettings;

        _twitchApi.Settings.ClientId = twitchSettings.Value.ClientId;
        _twitchApi.Settings.AccessToken = twitchSettings.Value.AccessToken;
    }

    public async Task<string?> GetBroadcasterUserId()
    {
        var users = await _twitchApi.Helix.Users.GetUsersAsync(
                    null,
                    new List<string> { _twitchSettings.Value.BroadcasterUserName });
        return users.Users.FirstOrDefault()?.Id;
    }
    public async Task<CustomReward?> GetRewardId(string rewardName)
    {
        var userId = await GetBroadcasterUserId();
        if (userId == null) return null;
        
        var rewards = await _twitchApi.Helix.ChannelPoints.GetCustomRewardAsync(userId);
        return rewards?.Data.FirstOrDefault(r => r.Title == rewardName);
    }

    public async Task SubscribeToChannelRedemptionAddEvents(string sessionId)
    {
        var userId = await GetBroadcasterUserId();
        if (userId == null) return;

        _ = await _twitchApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
            "channel.channel_points_custom_reward_redemption.add",
            "1",
            new Dictionary<string, string>() {
                                { "broadcaster_user_id", userId },
            },
            TwitchLib.Api.Core.Enums.EventSubTransportMethod.Websocket,
            sessionId
        );
    }

}
