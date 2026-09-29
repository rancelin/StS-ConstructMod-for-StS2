using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class ShiftStrike : AbstractCycleCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(null),
        HoverTipFactory.FromPower<DexterityPower>(null)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new PowerVar<StrengthPower>("Str", 1m),
        new PowerVar<DexterityPower>("Dex", -1m)
    ];

    public ShiftStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Shift Strike",
        "#*Cycle* if your *Dexterity* is negative.[br]Deal {Damage} damage.[br]Gain {Str} *Strength*. Lose {Dex} *Dexterity*.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        var dexterity = Owner.Creature.GetPower<DexterityPower>();
        return dexterity is { Amount: < 0 };
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars["Str"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature,
            DynamicVars["Dex"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["Str"].UpgradeValueBy(1m);
        DynamicVars["Dex"].UpgradeValueBy(-1m);
    }
}
