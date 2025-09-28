using Newtonsoft.Json.Linq;

namespace TCGStreamPacks.Twitch;

public class TwitchWSMessage(string json)
{
    public JToken RawMessage { get; } = JToken.Parse(json);
    public JToken Metadata => RawMessage["metadata"];
    public JToken Payload => RawMessage["payload"];
    public string MessageType => Metadata?["message_type"]?.ToString();
}