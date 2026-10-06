using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class PanicFire : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<PanicFirePower>(8m)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ConstructKeywords.Cycle),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public PanicFire() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Panic Fire",
        "#Whenever a non-Upgraded card *Cycles*, *Exhaust* it and deal {PanicFirePower} damage to a random enemy.{IfUpgraded:show:\nShuffle 3 random Cores into your draw pile.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PanicFirePower>(choiceContext, Owner.Creature,
            DynamicVars["PanicFirePower"].BaseValue, Owner.Creature, this);
        if (!IsUpgraded) return;
        if (CombatState is not { } combat) return;
        for (int i = 0; i < 3; i++)
        {
            CardModel canonical = ConstructCores.RandomCore(Owner);
            CardModel core = combat.CreateCard(canonical, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(core, PileType.Draw, Owner, CardPilePosition.Random);
        }
    }
}
