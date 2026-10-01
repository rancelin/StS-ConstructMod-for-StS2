using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Multistage : AbstractConstructCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Turns", 2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    public Multistage() : base(-1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return EnergyHoverTip;
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Multistage",
        "#Exhaust an Attack of cost [E] or less.\nAt the start of your next {Turns} turns, play a copy of that card.");

    protected override bool IsPlayable =>
        PileType.Hand.GetPile(Owner).Cards.Any(c => c != this && c.Type == CardType.Attack);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = ResolveEnergyXValue();
        if (x <= 0) return;

        var candidates = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c != this && c.Type == CardType.Attack
                && c.EnergyCost.GetWithModifiers(CostModifiers.All) <= x)
            .ToList();
        if (candidates.Count == 0) return;

        CardModel? chosen = candidates.Count == 1
            ? candidates[0]
            : (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(
                    new LocString("cards", "CONSTRUCTMOD-MULTISTAGE.selectionScreenPrompt"), 1),
                context: choiceContext, player: Owner,
                filter: c => c != this && c.Type == CardType.Attack && c.EnergyCost.GetWithModifiers(CostModifiers.All) <= x,
                source: this)).FirstOrDefault();
        if (chosen == null) return;

        await PowerCmd.Apply<MultistagePower>(choiceContext, Owner.Creature,
            DynamicVars["Turns"].IntValue, Owner.Creature, chosen);
        var power = Owner.Creature.Powers.OfType<MultistagePower>()
            .FirstOrDefault(p => p.Amount == DynamicVars["Turns"].IntValue);
        power?.SetHeldCard(chosen);
        await CardCmd.Exhaust(choiceContext, chosen);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Turns"].UpgradeValueBy(1m);
    }
}
