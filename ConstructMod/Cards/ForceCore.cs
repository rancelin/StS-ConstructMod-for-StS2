using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class ForceCore : AbstractCoreCard
{
    protected override string CoreName => "Force Core";
    protected override CardModel CanonicalCore => ModelDb.Card<ForceCore>();
    protected override string CoreCycleText => "Gain {Str} Strength until the end of this turn.";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Str", 1m)];

    public ForceCore() : base(CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCoreEffect(choiceContext);
    }

    protected override async Task OnCoreCycle(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<ForceCorePower>(choiceContext, Owner.Creature,
            DynamicVars["Str"].BaseValue, Owner.Creature, this);
    }
}
