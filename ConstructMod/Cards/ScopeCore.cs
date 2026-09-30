using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class ScopeCore : AbstractCoreCard
{
    protected override string CoreName => "Scope Core";
    protected override string CoreCycleText => "A random enemy gains {Vuln} Vulnerable.";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Vuln", 1m)];

    public ScopeCore() : base(CardType.Skill, TargetType.AllEnemies)
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
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
            DynamicVars["Vuln"].BaseValue, Owner.Creature, this);
    }
}
