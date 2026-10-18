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
    public override int IntrinsicMaxUpgradeLevel => 2;
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
        "#Deal !Damage! damage.\nDraw !Cards! cards.\n*Overheat*: {Overheat}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +1 card drawn (2 → 3). Mega: +2 damage AND +2 more cards
        // (StS1 M_UPGRADE_PLUS_ATTACK_DMG / M_UPGRADE_PLUS_DRAW).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Cards.UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
            DynamicVars.Cards.UpgradeValueBy(2m);
        }
    }
}
