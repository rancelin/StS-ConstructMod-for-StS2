using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class MassProduction : AbstractConstructCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    public MassProduction() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Mass Production",
        "#Choose a non-*Rare* card in your draw pile. *Exhaust* your hand and replace it with copies of that card.");

    protected override bool IsPlayable =>
        PileType.Draw.GetPile(Owner).Cards.Any(c => c.Rarity != CardRarity.Rare);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat) return;
        var candidates = PileType.Draw.GetPile(Owner).Cards
            .Where(c => c.Rarity != CardRarity.Rare).ToList();
        if (candidates.Count == 0) return;

        CardModel? chosen = candidates.Count == 1
            ? candidates[0]
            : (await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, Owner,
                new CardSelectorPrefs(new LocString("cards", "CONSTRUCTMOD-MASSPRODUCTION.selectionScreenPrompt"), 1)
            )).FirstOrDefault();
        if (chosen == null) return;

        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        foreach (var c in hand)
        {
            await CardCmd.Exhaust(choiceContext, c);
        }
        foreach (var c in hand)
        {
            var copy = combat.CreateCard(chosen, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
