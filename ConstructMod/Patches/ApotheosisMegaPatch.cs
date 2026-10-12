using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using ConstructMod.Cards;

namespace ConstructMod.Patches;

/// <summary>
/// Adaptation: vanilla <see cref="Apotheosis"/> ("Upgrade ALL of your cards") only upgrades
/// cards that pass its eligibility — and the Construct's mega gating caps player-chosen
/// upgrades at the regular tier in combat, so already-upgraded (+1) Construct cards would be
/// skipped entirely, staying +1 while the rest of the deck upgrades. This tops up every
/// ALREADY-UPGRADED Construct card that has room to grow to its full (mega) tier, making
/// Apotheosis feel right for the character. Non-upgraded Construct cards still only get the
/// vanilla single regular upgrade from Apotheosis's own pass (mega remains something you invest
/// in first). Non-Construct cards are untouched.
///
/// The eligibility snapshot MUST be a prefix: a postfix — even code placed before its
/// `await __result` — runs after vanilla's synchronous section, by which point Apotheosis
/// has already taken 0→+1 cards to +1, making them indistinguishable from genuinely
/// pre-upgraded ones (that bug shipped +2s straight from unupgraded). The prefix captures
/// true pre-play state per Apotheosis instance.
/// </summary>
[HarmonyPatch(typeof(Apotheosis), "OnPlay")]
public static class ApotheosisMegaPatch
{
    private static readonly ConditionalWeakTable<Apotheosis, HashSet<CardModel>> PreUpgraded = new();

    [HarmonyPrefix]
    static void SnapshotPreUpgraded(Apotheosis __instance)
    {
        var set = PreUpgraded.GetOrCreateValue(__instance);
        set.Clear();
        var cards = __instance.Owner?.PlayerCombatState?.AllCards;
        if (cards == null) return;
        foreach (var card in cards)
        {
            if (card is AbstractConstructCard construct
                && construct.IsUpgraded
                && construct.CurrentUpgradeLevel < construct.IntrinsicMaxUpgradeLevel)
            {
                set.Add(construct);
            }
        }
        ConstructModMain.Logger.Info($"Apotheosis: {set.Count} pre-upgraded Construct cards eligible for mega top-up.");
    }

    // OnPlay returns a Task; taking it as __result and awaiting makes this postfix run after the
    // vanilla upgrade pass has actually completed (Harmony's async-postfix pattern).
    [HarmonyPostfix]
    static async Task MegaTopUpPreUpgraded(Task __result, Apotheosis __instance)
    {
        await __result;
        if (!PreUpgraded.TryGetValue(__instance, out var set)) return;
        PreUpgraded.Remove(__instance);
        foreach (var card in set.ToList())
        {
            if (card is AbstractConstructCard construct
                && construct.CurrentUpgradeLevel < construct.IntrinsicMaxUpgradeLevel)
            {
                ConstructModMain.Logger.Info($"Apotheosis mega top-up: {construct.Title} ({construct.CurrentUpgradeLevel} -> max).");
                AbstractConstructCard.ForceUpgradeToMax(construct);
            }
        }
    }
}
