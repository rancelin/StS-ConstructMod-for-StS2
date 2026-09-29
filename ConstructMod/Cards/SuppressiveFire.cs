using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class SuppressiveFire : AbstractConstructCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BlurPower>(null)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new PowerVar<BlurPower>("Blur", 1m)
    ];

    public SuppressiveFire() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Suppressive Fire",
        "#Deal {Damage} damage to ALL enemies.[br]Gain {Blur} *Blur*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_fire", null, "fire_attack.mp3")
            .Execute(choiceContext);
        await PowerCmd.Apply<BlurPower>(choiceContext, Owner.Creature,
            DynamicVars["Blur"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
