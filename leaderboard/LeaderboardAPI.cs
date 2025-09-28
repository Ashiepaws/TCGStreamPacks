using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TCGStreamPacks.Leaderboard;

public static class LeaderboardAPI
{
    public static async Task<HttpResponseMessage> SubmitCard(string username, CardData card)
    {
        float marketValue = CPlayerData.GetCardMarketPrice(card);
        Plugin.Logger.LogInfo($"Card drawn for {username}: {card.monsterType} - {card.borderType} - {card.isFoil} - {marketValue}");

        throw new NotImplementedException();
    }
}