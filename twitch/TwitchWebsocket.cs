using System;
using Websocket.Client;

namespace TCGStreamPacks.Twitch;

public class TwitchWebsocket : WebsocketClient
{
    private readonly TwitchEventManager _parent;
    public string SessionID { get; private set; }

    public TwitchWebsocket(TwitchEventManager parent) : base(new Uri(Plugin.TwitchWebsocketUrl))
    {
        _parent = parent;
        MessageReceived.Subscribe(HandleMessage);
    }

    private void HandleMessage(ResponseMessage message)
    {
        Plugin.Logger.LogDebug($"Twitch WebSocket message received: {message.Text}");
        TwitchWSMessage wsMessage = new TwitchWSMessage(message.Text);
        if (wsMessage.MessageType == "session_welcome")
        {
            SessionID = wsMessage.Payload?["session"]?["id"]?.ToString();
            Plugin.Logger.LogDebug($"Twitch WebSocket session ID: {SessionID}");
        }
        _parent.HandleMessage(wsMessage);
    }
}