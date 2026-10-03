using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Hooks;

/// <summary>
/// Implement on powers (and potentially relics) that want to react whenever a card Overheats
/// (turns into a Burn because the per-turn cycle count reached the card's overheat threshold).
/// Dispatched via <see cref="CycleHook.AfterCardOverheated"/> (BaseLib <c>HookUtils.Dispatch</c>).
/// The <paramref name="overheatedCard"/> argument is the original card that was replaced by a Burn
/// (it has already been removed from its pile by the time the hook fires).
/// </summary>
public interface IAfterCardOverheated
{
    Task AfterCardOverheated(PlayerChoiceContext ctx, CardModel overheatedCard);
}
