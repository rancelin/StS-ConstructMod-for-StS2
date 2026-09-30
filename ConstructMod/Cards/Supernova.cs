using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Factories;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Supernova : AbstractConstructCard
{
    public Supernova() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Supernova",
        "#Exhaust ALL *Status* cards and replace them with random{IfUpgraded:show: *upgraded*|} cards.\nThose cards cost 0 this combat.");

    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } pcs || CombatState is not { } combat) return;
        var pools = new[] { Owner.Character.CardPool };
        var options = CardFactory.FilterForCombat(pools[0].AllCards).ToList();
        foreach (var pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            var statuses = pileType.GetPile(Owner).Cards.Where(c => c.Type == CardType.Status).ToList();
            foreach (var status in statuses)
            {
                await CardPileCmd.RemoveFromCombat(status);
                var canonical = Owner.RunState.Rng.CombatCardGeneration.NextItem(options);
                if (canonical == null) continue;
                var replacement = combat.CreateCard(canonical, Owner);
                if (IsUpgraded) CardCmd.Upgrade(replacement);
                replacement.EnergyCost.AddThisCombat(-9, reduceOnly: true);
                await CardPileCmd.AddGeneratedCardToCombat(replacement, pileType, Owner, CardPilePosition.Top);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
