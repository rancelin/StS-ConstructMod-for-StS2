using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class FlakBarrage : AbstractCycleCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    IsUpgraded
        ?
        [
            HoverTipFactory.FromPower<StrengthPower>(null)
        ]
        : Array.Empty<IHoverTip>();

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords)
            {
                if (k != ConstructKeywords.Cycle || IsUpgraded) yield return k;
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(0m, ValueProp.Move),
        new DynamicVar("Hits", 4m)
    ];

    public FlakBarrage() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Flak Barrage",
        "#Deal !Damage! damage to a random enemy {Hits} times.{IfUpgraded:show:\\n*Cycle* if your *Strength* is 0 or less.|}");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        if (!IsUpgraded) return false;
        return !(Owner.Creature.GetPower<StrengthPower>()?.Amount > 0m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (var i = 0; i < (int)DynamicVars["Hits"].BaseValue; i++)
        {
            if (CombatState is not { } combat || !combat.HittableEnemies.Any()) break;
            var enemy = Owner.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies);
            if (enemy == null) break;
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
