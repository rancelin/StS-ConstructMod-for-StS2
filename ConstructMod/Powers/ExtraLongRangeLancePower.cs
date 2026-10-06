using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// Applied by the upgraded <see cref="Cards.LongRangeLance"/> card as in-combat visual
/// feedback. The actual effect is carried by the <see cref="Relics.ExtraLongRangeLanceRelic"/>
/// the card grants on play — StS2's AfterCombatVictory hook never reaches powers, so the
/// relic is granted directly (see <see cref="LongRangeLancePower"/>).
/// </summary>
public class ExtraLongRangeLancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "EXTRA-Long-Range Lance",
        Description: "At the start of the combat after your next one, deal {Amount} damage to a random enemy.",
        SmartDescription: "At the start of the combat after your next one, deal {Amount} damage to a random enemy.");
}
