namespace GenericHostConsoleApp;

internal sealed class OBSSettings
{
    public string Url { get; init; } = "ws://127.0.0.1:4455";
    public string Password { get; init; } = string.Empty;
}
internal sealed class OBSSettingsRewards
{
    public string RewardName { get; init; } = string.Empty;
    public string SceneName { get; init; } = string.Empty;
    public string SourceName { get; init; } = string.Empty;
    public string FilterName { get; init; } = string.Empty;
    public int Timeout { get; init; } = 5000;
    public bool Activate { get; init; } = true;
    public bool Deactivate { get; init; } = false;
    public bool Immediately { get; init; } = false;
}