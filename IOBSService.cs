using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace GenericHostConsoleApp;
internal interface IOBSService
{
    //public void GetBroadcasterUserId(string userName);
    Task OBSShowHideReward(OBSSettingsRewards reward);
    bool Connect();
    public OBSSettingsRewards? GetRewardByName(string rewardName);

    void RewardRedemption(string rewardTitle);
}