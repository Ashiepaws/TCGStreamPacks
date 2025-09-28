using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using TCGStreamPacks.Twitch;
using HarmonyLib;

namespace TCGStreamPacks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static TwitchEventManager TwitchEventManager { get; private set; }
    public static PackOpeningQueue PackOpeningQueue { get; } = new();

    // Configuration entries
    private static readonly Dictionary<ECollectionPackType, ConfigEntry<string>> PackTypeRewardNames = [];
    private static ConfigEntry<string> _TwitchClientId;
    private static ConfigEntry<string> _TwitchTokenScopes;
    private static ConfigEntry<string> _TwitchWebsocketUrl;
    private static ConfigEntry<string> _TwitchEventSubUrl;
    private static ConfigEntry<string> _LeaderboardApiUrl;
    private static ConfigEntry<bool> _OpenBrowserOnAuth;
    private static ConfigEntry<bool> _IgnoreSSL;

    public static string TwitchClientID => _TwitchClientId.Value;
    public static string TwitchTokenScopes => _TwitchTokenScopes.Value;
    public static string TwitchWebsocketUrl => _TwitchWebsocketUrl.Value;
    public static string TwitchEventSubUrl => _TwitchEventSubUrl.Value;
    public static string LeaderboardApiUrl => _LeaderboardApiUrl.Value;
    public static bool OpenBrowserOnAuth => _OpenBrowserOnAuth.Value;
    public static bool IgnoreSSL => _IgnoreSSL.Value;

    public static ECollectionPackType GetPackTypeForRewardName(string rewardName)
    {
        foreach (var kvp in PackTypeRewardNames)
        {
            if (kvp.Value.Value.Equals(rewardName, System.StringComparison.OrdinalIgnoreCase))
                return kvp.Key;
        }
        return ECollectionPackType.None;
    }

    private void Awake()
    {
        Logger = base.Logger;

        // Initialize BepInEx configuration
        InitConfig();

        // Initialize Twitch Event Manager
        TwitchEventManager.Initialize().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Logger.LogError("Failed to initialize Twitch Event Manager: " + task.Exception);
            }
            else
            {
                TwitchEventManager = task.Result;
                Logger.LogInfo("Twitch Event Manager initialized successfully.");
            }
        });

        // Initialize Harmony patches
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        // All done!
        Logger.LogInfo($"Awoo! {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    private void InitConfig()
    {
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
        _TwitchWebsocketUrl = Config.Bind("Twitch Connection", "WebSocket URL", "wss://eventsub.wss.twitch.tv/ws", "The WebSocket URL for Twitch EventSub. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Twitch WebSocket URL: {TwitchWebsocketUrl}");
        _TwitchEventSubUrl = Config.Bind("Twitch Connection", "EventSub URL", "https://api.twitch.tv/helix/eventsub/subscriptions", "The HTTP URL for Twitch EventSub. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Twitch EventSub URL: {TwitchEventSubUrl}");

        // Leaderboard Connection config
        _LeaderboardApiUrl = Config.Bind("Leaderboard Connection", "API URL", "https://api.ashiepaws.dev/tcgstreampacks/v1", "The base URL for the TCG Leaderboard API. Don't touch this if you don't know what it is.");
        Logger.LogDebug($"Leaderboard API URL: {LeaderboardApiUrl}");

        // Other configs
        _OpenBrowserOnAuth = Config.Bind("Twitch Connection", "Open Browser On Auth", true, "If true, the browser will be opened automatically when authentication is required. If false, you will need to open the URL manually.");
        _IgnoreSSL = Config.Bind("Twitch Connection", "Ignore SSL Errors", false, "If true, SSL certificate errors will be ignored when connecting to Twitch and the Leaderboard API. This is insecure and should only be used for debugging.");
    }
}
