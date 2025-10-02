using HarmonyLib;
using TCGStreamPacks;
using TMPro;

[HarmonyPatch]
public class CardOpeningUIPatch
{
    private static bool _startedShowingTotalValue = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CardOpeningSequenceUI), "ShowSingleCardValue")]
    static void ShowSingleCardOwner(float cardValue, TextMeshProUGUI ___m_CardValueText)
    {
        string username = Plugin.PackOpeningQueue.CurrentPackOpener;
        if (!string.IsNullOrEmpty(username))
            ___m_CardValueText.text += $" ({username})";
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CardOpeningSequenceUI), "Update")]
    static void ShowTotalValueOwner(TextMeshProUGUI ___m_TotalCardValueText, bool ___m_IsShowingTotalValue)
    {
        string username = Plugin.PackOpeningQueue.CurrentPackOpener;
        if (___m_IsShowingTotalValue && !___m_TotalCardValueText.text.Contains('('))
        {
            _startedShowingTotalValue = true;
            if (!string.IsNullOrEmpty(username))
                ___m_TotalCardValueText.text += $" ({username})";
        }
        else if (!___m_IsShowingTotalValue && _startedShowingTotalValue)
        {
            _startedShowingTotalValue = false;
            if (!string.IsNullOrEmpty(username))
                ___m_TotalCardValueText.text += $" ({username})";
        }
    }
}