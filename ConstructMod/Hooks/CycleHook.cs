using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Hooks;

/// <summary>
/// Dispatcher for the <see cref="IAfterCardCycled"/> hook. Mirrors Downfall's
/// <c>DownfallHook</c> / BaseLib's <c>BaseLibHooks</c>: a one-liner over
/// <see cref="HookUtils.Dispatch{T}"/> that iterates all active combat hook listeners.
/// </summary>
public static class CycleHook
{
    public static Task AfterCardCycled(ICombatState? combatState, PlayerChoiceContext ctx, CardModel card)
        => HookUtils.Dispatch<IAfterCardCycled>(combatState, ctx, m => m.AfterCardCycled(ctx, card));
}
