using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Hooks;

namespace ConstructMod.Powers;

public class PointDefensePower : CustomPowerModel, IAfterCardCycled
{
    private int _limit;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Point Defense",
        Description: "#Whenever a card Cycles, gain 1 Block. Works {Amount} time(s) per turn.",
        SmartDescription: "#Whenever a card Cycles, gain 1 Block, a limited number of times per turn.");

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this && amount > 0)
        {
            _limit = Amount;
        }
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // NOTE: must NOT reset here — AfterPlayerTurnStart fires after the draw phase
        // (cf. AbstractCycleCard's AfterPlayerTurnStartEarly comment), so resetting here
        // wipes the draw-phase consumption (5→N→5 bug). Reset happens in AfterSideTurnEnd.
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        // Refresh the per-turn pool at the end of our turn (CycleCount.ResetTurn pattern),
        // so the next turn's draw phase starts full and both phases share one pool.
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            SetAmount(_limit);
        }
        return Task.CompletedTask;
    }

    public async Task AfterCardCycled(PlayerChoiceContext ctx, CardModel card)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        if (Owner.CombatState == null) return;
        if (Amount <= 0) return;
        SetAmount(Amount - 1);
        Flash();
        await CreatureCmd.GainBlock(Owner, 1, ValueProp.Unpowered, null, fast: true);
    }
}
