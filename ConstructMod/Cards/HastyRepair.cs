using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class HastyRepair : AbstractConstructCard
{
    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Heal", 12m)];

    public HastyRepair() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Hasty Repair",
        "#Heal {Heal} HP. Lose 2 Max HP.\n{IfUpgraded:show:Put a copy of this card into your hand.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue);
        await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature, 2m, isFromCard: true);
        if (IsUpgraded && CombatState is { } combat)
        {
            var copy = combat.CreateCard(this, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner, MegaCrit.Sts2.Core.Entities.Cards.CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(4m);
    }
}
