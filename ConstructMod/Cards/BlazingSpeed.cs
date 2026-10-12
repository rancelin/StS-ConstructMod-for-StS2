using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Hooks;

namespace ConstructMod.Cards;

public class BlazingSpeed : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(3m, ValueProp.Move), new CardsVar(2), new OverheatVar()];

    public BlazingSpeed() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        Overheat = 5;
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Blazing Speed",
        "#Deal !Damage! damage.\nDraw {Cards} cards.\n*Overheat*: {Overheat}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        // Original: +1 draw on upgrade, then mega = +2 dmg AND +2 draw.
        // Folded: +2 dmg AND +3 draw (combined deltas).
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Cards.UpgradeValueBy(3m);
    }
}
