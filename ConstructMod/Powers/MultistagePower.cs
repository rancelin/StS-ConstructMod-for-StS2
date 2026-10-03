using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class MultistagePower : CustomPowerModel
{
    private CardModel? _heldCard;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Expose {Card} as a DynamicVar so PowerModel.HoverTips injects it into the SmartFormat variables
    // dict. This is the vanilla NightmarePower pattern: the StringVar's StringValue holds the
    // held card's title and is what {Card} resolves to.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("Card", "")];

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Multistage",
        Description: "At the start of your next {Amount} turns, play a copy of {Card}.",
        SmartDescription: "At the start of your next turn, play a copy of {Card}.");

    public override Task BeforeApplied(Creature target, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        // cardSource is the exhausted attack chosen by the Multistage card; store it as the held card
        // and surface its title via the {Card} placeholder.
        if (cardSource != null && _heldCard == null) SetHeldCard(cardSource);
        return Task.CompletedTask;
    }

    public void SetHeldCard(CardModel card)
    {
        _heldCard = card;
        if (DynamicVars.ContainsKey("Card"))
        {
            ((StringVar)DynamicVars["Card"]).StringValue = card.Title;
        }
    }

    public string HeldCardName => _heldCard?.Title ?? "a card";

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || _heldCard == null) return;
        if (CombatState is not { } combat) return;
        Flash();

        ConstructModMain.Logger.Info($"MultistagePower: auto-playing copy of '{_heldCard.Title}' (cycles this turn = {CycleCount.GetCyclesThisTurn(Owner.Player)}).");

        // _heldCard is a mutable in-combat card; CreateClone preserves its upgrade state and produces
        // a proper in-combat copy. (combat.CreateCard requires a canonical model.)
        var copy = _heldCard.CreateClone();
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
