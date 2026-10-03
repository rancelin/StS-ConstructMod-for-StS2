using BaseLib.Abstracts;
using ConstructMod.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ConstructMod.Powers;

/// <summary>
/// 1-turn Thorns wrapper for the Electric Armor card. Uses BaseLib's
/// <see cref="CustomTemporaryPowerModelWrapper{TModel, TPower}"/> (the same pattern as Downfall's
/// Piercing Hide / Slime Spikes): the wrapper internally applies vanilla <see cref="ThornsPower"/>
/// to the owner and auto-removes it at the end of the enemy's turn
/// (<see cref="UntilEndOfOtherSideTurn"/> = true). No custom reflect hook, no manual removal.
/// </summary>
public class ElectricArmorThornsPower : CustomTemporaryPowerModelWrapper<ElectricArmor, ThornsPower>
{
    // Expire at the end of the enemy's turn (the "other side" relative to the player who played
    // Electric Armor). This makes the Thorns last exactly long enough to reflect during the
    // incoming enemy attacks, then clean up — matching the original "this turn" semantics.
    protected override bool UntilEndOfOtherSideTurn => true;
}
