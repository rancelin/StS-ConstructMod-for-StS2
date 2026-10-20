using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Hooks;

/// <summary>
/// Dispatcher for the cycle/overheat hooks. Mirrors Downfall's <c>DownfallHook</c> /
/// BaseLib's <c>BaseLibHooks</c>: one-liners over <see cref="HookUtils.Dispatch{T}"/> that
/// iterate all active combat hook listeners.
/// </summary>
public static class CycleHook
{
    public static Task AfterCardCycled(ICombatState? combatState, PlayerChoiceContext ctx, CardModel card)
        => HookUtils.Dispatch<IAfterCardCycled>(combatState, ctx, m => m.AfterCardCycled(ctx, card));

    public static Task AfterCardOverheated(ICombatState? combatState, PlayerChoiceContext ctx, CardModel overheatedCard)
        => HookUtils.Dispatch<IAfterCardOverheated>(combatState, ctx, m => m.AfterCardOverheated(ctx, overheatedCard));
}
