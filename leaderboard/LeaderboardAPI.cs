using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TCGStreamPacks.Leaderboard;

public static class LeaderboardAPI
{
    private static readonly HttpClient HttpClient;
    static LeaderboardAPI()
    {
        if (Plugin.IgnoreSSL)
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        HttpClient = new HttpClient();
    }

    public static async Task<HttpResponseMessage> SubmitCard(string username, CardData card)
    {
        float marketValue = CPlayerData.GetCardMarketPrice(card);
        Plugin.Logger.LogInfo($"Submitting card {card.monsterType} {card.borderType} {card.isFoil} (Value: {marketValue}) for user {username} to leaderboard of {Plugin.TwitchEventManager.BroadcasterId}.");

        var body = new
        {
            user = username,
            monster = card.monsterType.ToString(),
            border = card.borderType.ToString(),
            foil = card.isFoil,
            value = marketValue
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Plugin.LeaderboardApiUrl}/card");
        request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", $"{Plugin.TwitchEventManager.TwitchToken.AccessToken}");
        Plugin.Logger.LogDebug($"Sending leaderboard API request: {request}");
        var response = await HttpClient.SendAsync(request);
        Plugin.Logger.LogInfo($"Leaderboard API response: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        return response;
    }
}