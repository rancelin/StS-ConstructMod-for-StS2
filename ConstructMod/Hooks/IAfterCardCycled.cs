using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Hooks;

/// <summary>
/// Implement on powers, relics, or other models that want to react whenever a card Cycles.
/// Dispatched via <see cref="CycleHook.AfterCardCycled"/> (BaseLib <c>HookUtils.Dispatch</c>),
/// which iterates all active combat hook listeners (powers, relics, potions, cards, orbs) — no
/// manual subscribe/unsubscribe needed. Models are visited in the same order vanilla dispatches
/// <c>AfterCardDrawn</c> etc.
/// </summary>
public interface IAfterCardCycled
{
    Task AfterCardCycled(PlayerChoiceContext ctx, CardModel card);
}
