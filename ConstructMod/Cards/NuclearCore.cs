using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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
        [new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Poison", 2m), new OverheatVar()];

    public NuclearCore() : base(CardType.Skill, TargetType.Self)
    {
        Overheat = 10;
    }

    public override List<(string, string)>? Localization => new CardLoc(CoreName,
        $"#*Cycle*. When cycled: {CoreCycleText}\n*Overheat*: {{Overheat}}.\n{{IfUpgraded:show:Put a {CoreName} into your discard pile.|}}");

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
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
