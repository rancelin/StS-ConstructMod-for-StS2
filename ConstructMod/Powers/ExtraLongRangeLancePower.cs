using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using BaseLib.Abstracts;
using ConstructMod.Relics;

namespace ConstructMod.Powers;

/// <summary>
/// Applied by the upgraded <see cref="Cards.LongRangeLance"/> card. On combat victory, grants
/// the player an <see cref="ExtraLongRangeLanceRelic"/> — which skips the next combat and
/// replaces itself with a <see cref="LongRangeLanceRelic"/> so the damage lands at the start
/// of the combat after next.
/// </summary>
public class ExtraLongRangeLancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "EXTRA-Long-Range Lance",
        Description: "At the start of the combat after your next one, deal {Amount} damage to a random enemy.",
        SmartDescription: "At the start of the combat after your next one, deal {Amount} damage to a random enemy.");

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Owner.Player == null) return;
        Flash();
        var relic = await RelicCmd.Obtain<ExtraLongRangeLanceRelic>(Owner.Player);
        relic.SetLanceDamage(Amount);
    }
}
