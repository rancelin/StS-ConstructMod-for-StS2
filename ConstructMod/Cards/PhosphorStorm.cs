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
    public override int IntrinsicMaxUpgradeLevel => 2;
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
            // No Cycle tip: this card neither Cycles nor mentions Cycle in its text (only
            // *Overheat*, whose own tip already explains the Cycle interaction).
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Phosphor Storm",
        "#Deal !Damage! damage twice.{IfMega:mega: Deal !Damage! damage to a random enemy a third time.|}\n*Overheat*: {Overheat}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        // Two hits to the target, plus a third to a random enemy at the mega tier.
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).WithHitCount(2)
            .WithHitFx("vfx/vfx_molten_fist").Execute(choiceContext);
        if (IsMegaUpgraded && CombatState is { } combat)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
                .TargetingRandomOpponents(combat)
                .WithHitFx("vfx/vfx_molten_fist").Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +2 damage (7 → 9). Mega: -1 (9 → 8) but the third, random-enemy hit
        // is unlocked (see OnPlay; StS1 M_UPGRADE_PLUS_ATTACK_DMG is -1).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(-1m);
        }
    }
}
