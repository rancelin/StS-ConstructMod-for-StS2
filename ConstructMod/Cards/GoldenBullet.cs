using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class GoldenBullet : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18m, ValueProp.Move),
        new DynamicVar("Gold", 20m)
    ];

    public GoldenBullet() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Golden Bullet",
        "#Lose !Gold! gold.\nDeal !Damage! damage.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await PlayerCmd.LoseGold(DynamicVars["Gold"].BaseValue, Owner);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +4 damage and 5 less gold lost (20 → 15). Mega: +18 damage and
        // 35 more gold lost (15 → 50; StS1 M_UPGRADE_PLUS_ATTACK_DMG /
        // M_UPGRADE_PLUS_LOSE_GOLD_AMT).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
            DynamicVars["Gold"].UpgradeValueBy(-5m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Gold"].UpgradeValueBy(35m);
            DynamicVars.Damage.UpgradeValueBy(18m);
        }
    }
}
