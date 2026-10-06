using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// Applied by the <see cref="Cards.LongRangeLance"/> card as in-combat visual feedback
/// ("pending lance"). The actual effect is carried by the <see cref="Relics.LongRangeLanceRelic"/>
/// the card grants on play — StS2's AfterCombatVictory hook never reaches powers (the
/// run-level listener dispatch only visits relics/potions/cards/enchantments), so the StS1
/// "power spawns the relic on victory" pattern can't be replicated; the relic is granted
/// directly and fires at the start of the next combat instead.
/// </summary>
public class LongRangeLancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Long-Range Lance",
        Description: "At the start of your next combat, deal {Amount} damage to a random enemy.",
        SmartDescription: "At the start of your next combat, deal {Amount} damage to a random enemy.");
}
