using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Cards;
using ConstructMod.Cards;

namespace ConstructMod.Patches;

/// <summary>
/// Adaptation: vanilla <see cref="Apotheosis"/> ("Upgrade ALL of your cards") only upgrades
/// cards that pass <c>IsUpgradable</c> — and the Construct's mega gating caps player-chosen
/// upgrades at the regular tier in combat, so already-upgraded (+1) Construct cards would be
/// skipped entirely, staying +1 while the rest of the deck upgrades. This postfix tops up every
/// ALREADY-UPGRADED Construct card that has room to grow to its full (mega) tier, making
/// Apotheosis feel right for the character. Non-upgraded Construct cards still only get the
/// vanilla single regular upgrade from Apotheosis's own pass (mega remains something you invest
/// in first). Non-Construct cards are untouched.
/// </summary>
[HarmonyPatch(typeof(Apotheosis), "OnPlay")]
public static class ApotheosisMegaPatch
{
    // OnPlay returns a Task; taking it as __result and awaiting makes this postfix run after the
    // vanilla upgrade pass has actually completed (Harmony's async-postfix pattern).
    [HarmonyPostfix]
    static async Task MegaUpgradedConstructCards(Task __result, Apotheosis __instance)
    {
        await __result;
        var cards = __instance.Owner?.PlayerCombatState?.AllCards;
        if (cards == null) return;
        foreach (var card in cards.ToList())
        {
            if (card is AbstractConstructCard construct
                && construct.IsUpgraded
                && construct.CurrentUpgradeLevel < construct.IntrinsicMaxUpgradeLevel)
            {
                AbstractConstructCard.ForceUpgradeToMax(card);
            }
        }
    }
}
