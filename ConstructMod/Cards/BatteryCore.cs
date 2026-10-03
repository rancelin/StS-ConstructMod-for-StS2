using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Cards;

public class BatteryCore : AbstractCoreCard
{
    protected override string CoreName => "Battery Core";
    protected override CardModel CanonicalCore => ModelDb.Card<BatteryCore>();
    protected override string CoreCycleText => "A random card in your hand costs {Discount} less this turn.";

    protected override System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar> CanonicalVars =>
        [new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Discount", 1m)];

    public BatteryCore() : base(CardType.Skill, TargetType.Self)
    {
    }

    protected override Task OnCoreCycle(PlayerChoiceContext choiceContext)
    {
        var allHand = PileType.Hand.GetPile(Owner).Cards.ToList();
        var eligible = allHand.Where(c => c != this && c.EnergyCost.GetResolved() > 0).ToList();
        ConstructModMain.Logger.Info($"BatteryCore.OnCoreCycle: hand size={allHand.Count}, eligible={eligible.Count} ({string.Join(", ", allHand.Select(c => $"{c.Title}(cost={c.EnergyCost.GetResolved()})"))})");
        if (eligible.Count == 0) return Task.CompletedTask;
        var card = Owner.RunState.Rng.CombatTargets.NextItem(eligible) ?? eligible[0];
        if (card == null) return Task.CompletedTask;
        ConstructModMain.Logger.Info($"BatteryCore.OnCoreCycle: reducing cost of '{card.Title}' (resolved={card.EnergyCost.GetResolved()}) by {DynamicVars["Discount"].IntValue}.");
        // Relative reduction ("costs N less this turn") — the canonical pattern for this kind of effect.
        card.EnergyCost.AddThisTurnOrUntilPlayed(-DynamicVars["Discount"].IntValue, reduceOnly: true);
        var node = NCard.FindOnTable(card, null);
        if (node != null && card.Pile != null)
        {
            node.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
        }
        return Task.CompletedTask;
    }
}
