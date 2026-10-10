using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
namespace ConstructMod.Cards;
public class Implosion : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<Burn>()];

    public Implosion() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }
    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Implosion",
        "#Put ALL *Burns* from your exhaust pile into your hand.\nFor each, play a random card from your draw pile.");
    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
        }
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } pcs || CombatState is not { } combat) return;
        var burns = pcs.ExhaustPile.Cards.Where(c => c is Burn).ToList();
        foreach (var burn in burns)
        {
            if (PileType.Hand.GetPile(Owner).Cards.Count >= 10) break;
            await CardPileCmd.Add(burn, PileType.Hand, CardPilePosition.Top, skipVisuals: false);
            var playable = PileType.Draw.GetPile(Owner).Cards
                .Where(c => c is not Burn && !c.Keywords.Contains(CardKeyword.Unplayable))
                .ToList();
            if (playable.Count == 0) continue;
            var card = Owner.RunState.Rng.CombatTargets.NextItem(playable);
            if (card == null) continue;
            card.EnergyCost.SetUntilPlayed(0, reduceOnly: true);
            Creature? target = card.TargetType == TargetType.AnyEnemy
                ? Owner.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies)
                : null;
            await CardCmd.AutoPlay(choiceContext, card, target);
        }
    }
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
