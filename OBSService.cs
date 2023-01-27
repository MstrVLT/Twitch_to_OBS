using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using static System.Formats.Asn1.AsnWriter;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace GenericHostConsoleApp;

internal sealed class OBSService : IOBSService
{
    private readonly IConfiguration Configuration;
    private readonly ILogger<OBSService> _logger;

    private readonly OBSWebsocket _obs = new OBSWebsocket();
    
    private BlockingCollection<OBSSettingsRewards> _jobs = new BlockingCollection<OBSSettingsRewards>();

    public OBSService(
        ILogger<OBSService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        Configuration = configuration;

        var thread = new Thread(() =>
        {
            foreach (var job in _jobs.GetConsumingEnumerable(CancellationToken.None))
            {
                OBSShowHideReward(job).GetAwaiter().GetResult();
                Thread.Sleep(2000);
            }
        })
        {
            IsBackground = true
        };
        thread.Start();
    }

    public bool Connect()
    {
        var obsSettings = Configuration.GetSection("OBS")
            .Get<OBSSettings>();
        if(obsSettings == null) return false;
        
        var rewards = Configuration.GetSection("Rewards").Get<List<OBSSettingsRewards>>();  
        if(rewards == null) return false;
        
        _obs.ConnectAsync(obsSettings.Url,obsSettings.Password);

        if (!_obs.IsConnected) return _obs.IsConnected;
        
        _logger.LogInformation($"OBS Websocket {_obs.GetVersion()} connected!");
        
        return _obs.IsConnected;
    }
    
    public void RewardRedemption(string rewardTitle)
    {
        var reward = GetRewardByName(rewardTitle);
        if (reward == null) return;
        
        if (reward.Immediately)
            Task.Run(async () => await OBSShowHideReward(reward));
        else
            _jobs.Add(reward);
    }

    public OBSSettingsRewards? GetRewardByName(string rewardName)
    {
        var rewards = Configuration.GetSection("Rewards").Get<List<OBSSettingsRewards>>();  
        if(rewards == null) return null;
        
        return rewards.FirstOrDefault(r =>
            String.Equals(r.RewardName, rewardName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task OBSShowHideReward(OBSSettingsRewards reward)
    {
        if (reward.FilterName == "")
            await Task.Run(async () =>
            {
                if (reward.SceneName == "") return;
                var id = _obs.GetSceneItemId(reward.SceneName, reward.SourceName, 0);
                _obs.SetSceneItemEnabled(reward.SceneName, id, reward.Activate);
                await Task.Delay(reward.Timeout);
                _obs.SetSceneItemEnabled(reward.SceneName, id, reward.Deactivate);
            });
        else
            await Task.Run(async () =>
            {
                _obs.SetSourceFilterEnabled(reward.SourceName, reward.FilterName, reward.Activate);
                await Task.Delay(reward.Timeout);
                _obs.SetSourceFilterEnabled(reward.SourceName, reward.FilterName, reward.Deactivate);
            });
    }
}
