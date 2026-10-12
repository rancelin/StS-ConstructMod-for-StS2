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

public class PhosphorStorm : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7m, ValueProp.Move), new OverheatVar()];

    public PhosphorStorm() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
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

    public override List<(string, string)>? Localization => new CardLoc("Phosphor Storm",
        "#Deal !Damage! damage twice.{IfUpgraded:show: Deal !Damage! damage to a random enemy a third time.|}\n*Overheat*: {Overheat}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        // Two hits to the target. (Mega folded into upgrade: a third hit to a random enemy.)
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).WithHitCount(2)
            .WithHitFx("vfx/vfx_molten_fist").Execute(choiceContext);
        if (IsUpgraded && CombatState is { } combat)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
                .TargetingRandomOpponents(combat)
                .WithHitFx("vfx/vfx_molten_fist").Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // Original: +2 dmg on upgrade, then mega = -1 dmg + 3rd random hit.
        // Folded: +1 dmg (net of the mega's -1) + the 3rd random-enemy hit.
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
