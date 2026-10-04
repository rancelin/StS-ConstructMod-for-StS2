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
/// Applied by the <see cref="Cards.LongRangeLance"/> card. On combat victory, grants the player
/// a <see cref="LongRangeLanceRelic"/> whose damage counter equals this power's amount — the
/// relic then deals that damage at the start of the next combat and consumes itself.
/// </summary>
public class LongRangeLancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Long-Range Lance",
        Description: "At the start of your next combat, deal {Amount} damage to a random enemy.",
        SmartDescription: "At the start of your next combat, deal {Amount} damage to a random enemy.");

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Owner.Player == null) return;
        Flash();
        var relic = await RelicCmd.Obtain<LongRangeLanceRelic>(Owner.Player);
        relic.SetLanceDamage(Amount);
    }
}
