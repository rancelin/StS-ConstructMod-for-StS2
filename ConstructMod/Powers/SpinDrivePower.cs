using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class SpinDrivePower : CustomPowerModel
{
    private int _limit;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Spin Drive",
        Description: "Whenever you play a card, draw a card. Works {Limit} time(s) per turn.",
        SmartDescription: "Whenever you play a card, draw a card, a limited number of times per turn.");

    public override Task BeforeApplied(MegaCrit.Sts2.Core.Entities.Creatures.Creature target, decimal amount,
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, CardModel? cardSource)
    {
        _limit = (int)amount;
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, CardModel? cardSource)
    {
        if (power == this && amount > 0) _limit = Amount;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner.Player == player) SetAmount(_limit);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return;
        if (Amount <= 0) return;
        SetAmount(Amount - 1);
        Flash();
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, 1, cardPlay.Player, cardPlay.Card);
    }
}
