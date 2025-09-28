using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TCGStreamPacks.Leaderboard;

public static class LeaderboardAPI
{
    public static async Task<HttpResponseMessage> SubmitCard(string username, CardData card)
    {
        float marketValue = CPlayerData.GetCardMarketPrice(card);
        Plugin.Logger.LogInfo($"Submitting card {card.monsterType} {card.borderType} {card.isFoil} (Value: {marketValue}) for user {username} to leaderboard of {Plugin.TwitchEventManager.BroadcasterId}.");

        throw new NotImplementedException();
    }
}