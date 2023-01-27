using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace GenericHostConsoleApp;


internal interface ITwitchService
{
    public Task<string?> GetBroadcasterUserId();
    public Task<CustomReward?> GetRewardId(string rewardName);
    public Task SubscribeToChannelRedemptionAddEvents(string sessionId);

}