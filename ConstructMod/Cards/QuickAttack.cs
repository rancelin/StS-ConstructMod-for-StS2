using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ConstructMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class QuickAttack : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<DexterityPower>(null)];

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("Dex", 2m),
        new DynamicVar("DexLoss", 2m)
    ];

    public QuickAttack() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Quick Attack",
        "#Deal !Damage! damage.\nGain {Dex} *Dexterity*.\nAt the end of your turn, lose {DexLoss} *Dexterity*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        // StS1 gains `Dex` Dexterity and later loses `DexLoss` of it; the difference is a
        // permanent gain (0/0/+2 across the three tiers).
        var dex = DynamicVars["Dex"].BaseValue;
        var temp = DynamicVars["DexLoss"].BaseValue;
        if (dex > temp)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, dex - temp, Owner.Creature, this);
        }
        await PowerCmd.Apply<QuickAttackPower>(choiceContext, Owner.Creature, temp, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +2 damage / +1 Dexterity (lost again at end of turn). Mega: +3 damage /
        // +2 Dexterity, but the end-of-turn loss only grows by 1 — so 2 Dexterity sticks
        // (StS1 M_UPGRADE_PERMANENT_DEX_GAIN).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
            DynamicVars["Dex"].UpgradeValueBy(1m);
            DynamicVars["DexLoss"].UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
            DynamicVars["Dex"].UpgradeValueBy(2m);
        }
    }
}
