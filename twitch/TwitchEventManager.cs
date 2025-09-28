using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace TCGStreamPacks.Twitch;

public class TwitchEventManager
{
    private readonly TwitchWebsocket twitchWS;
    private readonly TwitchToken twitchToken;

    private TwitchEventManager(TwitchToken token)
    {
        twitchToken = token;
        twitchWS = new TwitchWebsocket(this);
        twitchWS.Start();
    }

    public static async Task<TwitchEventManager> Initialize()
    {
        var token = await TwitchToken.Create();
        return new TwitchEventManager(token);
    }

    private async void RegisterEventSub()
    {
        string broadcasterId = await TwitchAPI.GetUserId(twitchToken);
        Plugin.Logger.LogDebug($"Registering EventSub for broadcaster ID: {broadcasterId}");
        var registerBody = new
        {
            type = "channel.channel_points_custom_reward_redemption.add",
            version = "1",
            condition = new
            {
                broadcaster_user_id = broadcasterId
            },
            transport = new
            {
                method = "websocket",
                session_id = twitchWS.SessionID
            }
        };
        var response = await TwitchAPI.RegisterEventSub(twitchToken, registerBody);
        response.EnsureSuccessStatusCode();
        Plugin.Logger.LogDebug("Twitch EventSub subscription registered successfully.");
    }

    internal void HandleMessage(TwitchWSMessage message)
    {
        switch(message.MessageType)
        {
            case "session_welcome":
                Plugin.Logger.LogInfo("Twitch WebSocket session established.");
                RegisterEventSub();
                break;
            case "notification":
                var eventType = message.Payload?["subscription"]?["type"]?.ToString();
                if (eventType == "channel.channel_points_custom_reward_redemption.add")
                {
                    var rewardData = message.Payload?["event"];
                    var userName = rewardData?["user_name"]?.ToString();
                    var rewardTitle = rewardData?["reward"]?["title"]?.ToString();
                    var packType = Plugin.GetPackTypeForRewardName(rewardTitle);
                    if (packType != ECollectionPackType.None)
                    {
                        Plugin.Logger.LogInfo($"Enqueuing pack opening for {userName}: {packType}");
                        Plugin.PackOpeningQueue.EnqueuePackOpening(packType, userName);
                    }
                    else
                        Plugin.Logger.LogWarning($"No pack type configured for reward: {rewardTitle}");
                }
                break;
            default:
                Plugin.Logger.LogWarning($"Unhandled Twitch WS message type: {message.MessageType}");
                break;
        }
    }
}