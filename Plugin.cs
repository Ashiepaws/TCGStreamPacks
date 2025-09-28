using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace TCGStreamPacks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    // Configuration entries
    private static readonly Dictionary<ECollectionPackType, ConfigEntry<string>> PackTypeRewardNames = [];
    private static ConfigEntry<string> _TwitchClientId;
    private static ConfigEntry<string> _TwitchTokenScopes;
    private static ConfigEntry<string> _TwitchWebsocketUrl;
    private static ConfigEntry<string> _TwitchEventSubUrl;
    private static ConfigEntry<string> _LeaderboardApiUrl;

    public static string TwitchClientID => _TwitchClientId.Value;
    public static string TwitchTokenScopes => _TwitchTokenScopes.Value;
    public static string TwitchWebsocketUrl => _TwitchWebsocketUrl.Value;
    public static string TwitchEventSubUrl => _TwitchEventSubUrl.Value;
    public static string LeaderboardApiUrl => _LeaderboardApiUrl.Value;

    public static string GetRewardNameForPackType(ECollectionPackType packType)
    {
        if (PackTypeRewardNames.TryGetValue(packType, out ConfigEntry<string> rewardNameConfig))
            return rewardNameConfig.Value;
        return null;
    }

    private void Awake()
    {
        Logger = base.Logger;

        Logger.LogInfo("Initializing config...");

        // Pack type reward name configs
        foreach (ECollectionPackType packType in (ECollectionPackType[])System.Enum.GetValues(typeof(ECollectionPackType)))
        {
            if (packType == ECollectionPackType.None)
                continue;
            ConfigEntry<string> rewardNameConfig = Config.Bind("Twitch Reward Names", packType.ToString(), "", "The exact name of the Twitch reward that should enqueue a viewer for an opening of this pack type. Leave blank to disable.");
            Logger.LogDebug($"Config for {packType}: {rewardNameConfig.Value}");
            PackTypeRewardNames[packType] = rewardNameConfig;
        }

        // Twitch Connection config
        _TwitchClientId = Config.Bind("Twitch Connection", "Client ID", "xbahm4n1kwnls0d27tt9z7zqcc74vf", "The Client ID of the application to use when connecting to Twitch. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Twitch Client ID: {TwitchClientID}");
        _TwitchTokenScopes = Config.Bind("Twitch Connection", "Token Scopes", "channel:read:redemptions channel:manage:redemptions", "The OAuth token scopes to request when connecting to Twitch. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Twitch Token Scopes: {TwitchTokenScopes}");
        _TwitchWebsocketUrl = Config.Bind("Twitch Connection", "WebSocket URL", "wss://eventsub.wss.twitch.tv", "The WebSocket URL for Twitch EventSub. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Twitch WebSocket URL: {TwitchWebsocketUrl}");
        _TwitchEventSubUrl = Config.Bind("Twitch Connection", "EventSub URL", "https://api.twitch.tv/helix/eventsub", "The HTTP URL for Twitch EventSub. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Twitch EventSub URL: {TwitchEventSubUrl}");

        // Leaderboard Connection config
        _LeaderboardApiUrl = Config.Bind("Leaderboard Connection", "API URL", "https://api.ashiepaws.dev/tcgstreampacks/v1", "The base URL for the TCG Leaderboard API. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Leaderboard API URL: {LeaderboardApiUrl}");

        Logger.LogInfo($"Awoo! {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }
}
