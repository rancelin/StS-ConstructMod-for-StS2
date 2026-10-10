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

namespace ConstructMod.Powers;

public class MultistagePower : CustomPowerModel
{
    private sealed class StagedCard
    {
        public CardModel Card;
        public int TurnsLeft;
        public StagedCard(CardModel card, int turns)
        {
            Card = card;
            TurnsLeft = turns;
        }
    }

    private readonly List<StagedCard> _staged = [];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Expose {Card} as a DynamicVar so PowerModel.HoverTips injects it into the SmartFormat variables
    // dict. This is the vanilla NightmarePower pattern: the StringVar's StringValue holds the
    // staged card title(s) and is what {Card} resolves to.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("Card", "")];

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Multistage",
        Description: "#At the start of your next {Amount} turns, play a copy of {Card}.",
        SmartDescription: "#At the start of your next {Amount} turns, play a copy of {Card}.");

    public override Task BeforeApplied(Creature target, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        // StS1 mints a separate power instance per play (unique ID suffix, "prevents
        // power stacking"); StS2 Counter powers merge instead, so each play is staged
        // as its own entry with its own remaining turns. Second plays no longer eat
        // the first card or merge turn counts.
        if (cardSource != null)
            _staged.Add(new StagedCard(cardSource, (int)amount));
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        // Re-assert the display after the engine merges the stacked amount: Amount
        // shows the longest remaining run, {Card} the staged titles.
        if (power == this) SyncDisplay();
        return Task.CompletedTask;
    }

    private void SyncDisplay()
    {
        if (_staged.Count == 0) return;
        var turns = _staged.Max(e => e.TurnsLeft);
        if (Amount != turns) SetAmount(turns);
        if (DynamicVars.ContainsKey("Card"))
        {
            var titles = _staged.Select(e => e.Card.Title).Distinct().ToList();
            ((StringVar)DynamicVars["Card"]).StringValue = string.Join(" and ", titles);
        }
    }

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        if (CombatState is not { } combat) return;
        if (_staged.Count == 0)
        {
            await PowerCmd.Remove(this);
            return;
        }
        Flash();

        foreach (var entry in _staged.ToList())
        {
            if (entry.TurnsLeft <= 0) continue;
            // The staged card is the exhausted attack; CreateClone preserves its upgrade
            // state and produces a proper in-combat copy. (combat.CreateCard requires a
            // canonical model.)
            var copy = entry.Card.CreateClone();
            copy.SetToFreeThisTurn();
            copy.ExhaustOnNextPlay = true;
            var target = combat.HittableEnemies.Any()
                ? Owner.Player.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies)
                : null;
            await CardPileCmd.Add(copy, PileType.Play);
            await CardCmd.AutoPlay(choiceContext, copy, target, skipXCapture: true);
            entry.TurnsLeft--;
        }
        _staged.RemoveAll(e => e.TurnsLeft <= 0);
        if (_staged.Count == 0)
            await PowerCmd.Remove(this);
        else
            SyncDisplay();
    }
}
