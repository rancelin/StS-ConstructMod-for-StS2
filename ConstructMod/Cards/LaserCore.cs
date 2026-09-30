using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class LaserCore : AbstractCoreCard
{
    protected override string CoreName => "Laser Core";
    protected override string CoreCycleText => "ALL enemies take !Damage! damage.";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(2m, ValueProp.Unpowered)];

    public LaserCore() : base(CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCoreEffect(choiceContext);
    }

    protected override async Task OnCoreCycle(PlayerChoiceContext choiceContext)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await CreatureCmd.Damage(choiceContext, enemy, DynamicVars.Damage.BaseValue,
                ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner.Creature);
        }
    }
}
