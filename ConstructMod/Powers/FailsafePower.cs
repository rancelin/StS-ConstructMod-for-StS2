using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class FailsafePower : CustomPowerModel
{
    private int _remaining;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Failsafe",
        Description: "The next {Amount} Status cards you draw each turn Cycle. Resets each turn.",
        SmartDescription: "The next {Amount} Status cards you draw each turn Cycle. Resets each turn.");

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this) _remaining = (int)Amount;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner.Player == player) _remaining = (int)Amount;
        return Task.CompletedTask;
    }

    public bool TryConsume()
    {
        if (_remaining <= 0) return false;
        _remaining--;
        return true;
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature != Owner) return;
        if (card.Type != CardType.Status) return;
        if (!TryConsume()) return;
        Flash();
        CycleCount.Increment(card.Owner);
        await CycleEvents.NotifyCycle(choiceContext, card);
        await CardCmd.DiscardAndDraw(choiceContext, [card], 1);
    }
}
