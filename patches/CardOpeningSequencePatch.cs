namespace TCGStreamPacks.Patches;

using System.Collections.Generic;
using HarmonyLib;
using TCGStreamPacks.Leaderboard;

[HarmonyPatch(typeof(CardOpeningSequence), "GetPackContent")]
public class CardOpeningSequencePatch
{
    static void Postfix(bool clearList, int godPackRollIndex, bool isSecondaryRolledData, ECollectionPackType overrideCollectionPackType, ref List<CardData> ___m_RolledCardDataList, ref ECollectionPackType ___m_CollectionPackType)
    {
        string username = Plugin.PackOpeningQueue.DequeuePackOpening(overrideCollectionPackType != ECollectionPackType.None ? overrideCollectionPackType : ___m_CollectionPackType);
        if(!string.IsNullOrEmpty(username))
        {
            foreach (var card in ___m_RolledCardDataList)
                LeaderboardAPI.SubmitCard(username, card).ContinueWith(task =>
                {
                    if (task.Exception != null)
                        Plugin.Logger.LogError("Failed to submit card to leaderboard: " + task.Exception);
                });
        }
    }
}
