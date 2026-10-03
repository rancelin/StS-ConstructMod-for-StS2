using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class NuclearCore : AbstractCoreCard
{
    protected override string CoreName => "Nuclear Core";
    protected override CardModel CanonicalCore => ModelDb.Card<NuclearCore>();
    protected override string CoreCycleText => "Apply {Poison} *Poison* to ALL enemies.";

    protected override System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar> CanonicalVars =>
        [new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Poison", 2m)];

    public NuclearCore() : base(CardType.Skill, TargetType.Self)
    {
        Overheat = 10;
    }

    protected override async Task OnCoreCycle(PlayerChoiceContext choiceContext)
    {
        if (CombatState is not { } combat) return;
        foreach (var enemy in combat.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy,
                DynamicVars["Poison"].IntValue, Owner.Creature, this);
        }
    }
}
