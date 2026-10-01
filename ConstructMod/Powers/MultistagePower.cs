using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class MultistagePower : CustomPowerModel
{
    private CardModel? _heldCard;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Multistage",
        Description: "At the start of your next {Amount} turns, play a copy of {Card}.",
        SmartDescription: "At the start of your next turn, play a copy of {Card}.");

    public override Task BeforeApplied(Creature target, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (cardSource != null && _heldCard == null) _heldCard = cardSource;
        return Task.CompletedTask;
    }

    public void SetHeldCard(CardModel card)
    {
        _heldCard = card;
    }

    public string HeldCardName => _heldCard?.Title ?? "a card";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || _heldCard == null) return;
        if (CombatState is not { } combat) return;
        Flash();

        var copy = combat.CreateCard(_heldCard, Owner.Player);
        copy.SetToFreeThisTurn();
        copy.ExhaustOnNextPlay = true;
        var target = combat.HittableEnemies.Any()
            ? Owner.Player.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies)
            : null;
        await CardPileCmd.Add(copy, PileType.Play);
        await CardCmd.AutoPlay(choiceContext, copy, target, skipXCapture: true);

        if (Amount <= 1)
            await PowerCmd.Remove(this);
        else
            SetAmount(Amount - 1);
    }
}
