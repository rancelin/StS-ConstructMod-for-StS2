using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// While active, globally suppresses the Overheat system — no card will Overheat (turn into a Burn
/// from cycling) as long as the player has this power. The overheat check reads
/// <see cref="MegaCrit.Sts2.Core.Entities.Creatures.Creature.HasPower{T}"/> for this type.
/// Amount = number of turns the freeze lasts. Skips the first turn-end after application
/// (justApplied idiom) so a 1-stack freeze actually freezes for one full enemy turn.
/// </summary>
public class FlashFreezePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Flash Freeze",
        Description: "Your cards cannot Overheat.",
        SmartDescription: "Your cards cannot Overheat for {Amount} more turn(s).");

    private bool _justApplied = true;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return;
        // Skip the first turn-end after application so the freeze survives the enemy turn.
        if (_justApplied)
        {
            _justApplied = false;
            return;
        }
        if (Amount > 1)
        {
            await PowerCmd.TickDownDuration(this);
            return;
        }
        await PowerCmd.Remove(this);
    }
}
