using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class FlameCore : AbstractCoreCard
{
    protected override string CoreName => "Flame Core";
    protected override CardModel CanonicalCore => ModelDb.Card<FlameCore>();
    protected override string CoreCycleText => "A random enemy takes !Damage! damage.";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(3m, ValueProp.Unpowered)];

    public FlameCore() : base(CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCoreEffect(choiceContext);
    }

    protected override async Task OnCoreCycle(PlayerChoiceContext choiceContext)
    {
        var enemy = Owner.RunState.Rng.CombatTargets.NextItem(CombatState!.HittableEnemies);
        if (enemy == null) return;
        await CreatureCmd.Damage(choiceContext, enemy, DynamicVars.Damage.BaseValue,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner.Creature);
    }
}
