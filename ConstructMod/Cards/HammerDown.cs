using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class HammerDown : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(null),
        HoverTipFactory.FromPower<DexterityPower>(null)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move)
    ];

    public HammerDown() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Hammer Down",
        "#Deal !Damage! damage.\n{IfMega:mega:Triple|Double} your *Strength* and *Dexterity*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        // StS1's DoubleStatsAction applies the current amount again ×1 (double) or ×2
        // (mega: triple).
        var mult = IsMegaUpgraded ? 2m : 1m;
        var str = Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0m;
        if (str != 0m)
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, str * mult, Owner.Creature, this);
        var dex = Owner.Creature.GetPower<DexterityPower>()?.Amount ?? 0m;
        if (dex != 0m)
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, dex * mult, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Regular: +4 damage (12 → 16). Mega: no stat change — Double becomes Triple
        // (see OnPlay); StS1's M_UPGRADE_NEW_COST line is commented out upstream.
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }
}
