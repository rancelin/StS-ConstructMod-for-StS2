using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;using MegaCrit.Sts2.Core.Entities.Powers;using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class PointDefensePower : CustomPowerModel
{
    private int _limit;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Point Defense",
        Description: "Whenever a card Cycles, gain 1 Block. Works {Limit} time(s) per turn.",
        SmartDescription: "Whenever a card Cycles, gain 1 Block, a limited number of times per turn.");

    public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _limit = (int)amount;
        CycleEvents.CardCycled += OnCardCycled;
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this && amount > 0)
        {
            _limit = Amount;
        }
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        CycleEvents.CardCycled -= OnCardCycled;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            SetAmount(_limit);
        }
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        CycleEvents.CardCycled -= OnCardCycled;
        return Task.CompletedTask;
    }
    private async Task OnCardCycled(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        if (Owner.CombatState == null) return;
        if (Amount <= 0) return;
        SetAmount(Amount - 1);
        Flash();
        await CreatureCmd.GainBlock(Owner, 1, ValueProp.Unpowered, null, fast: true);
    }
}
