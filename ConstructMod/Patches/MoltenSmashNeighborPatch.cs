using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Patches;

/// <summary>
/// Snapshots <see cref="Cards.MoltenSmash"/>'s hand neighbors BEFORE the card moves from the
/// hand pile to the play pile. <see cref="CardModel.OnPlayWrapper"/> calls
/// <c>CardPileCmd.AddDuringManualCardPlay</c> as its very first step, so by the time
/// <c>OnPlay</c> (and even <c>BeforeCardPlayed</c>) runs, the card is no longer in hand and its
/// hand position can't be queried. A Harmony prefix on <c>OnPlayWrapper</c> runs before the
/// pile move — the same approach Downfall uses for the Hermit "Dead On" mechanic
/// (its DeadOnPatch snapshots hand state off OnPlayWrapper's state machine).
/// The prefix is picked up by the mod's <c>PatchAll</c> in ConstructModMain.Initialize.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class MoltenSmashNeighborPatch
{
    [HarmonyPrefix]
    static void CacheHandNeighbors(CardModel __instance)
    {
        if (__instance is Cards.MoltenSmash smash)
        {
            smash.CacheHandNeighbors();
        }
    }
}
