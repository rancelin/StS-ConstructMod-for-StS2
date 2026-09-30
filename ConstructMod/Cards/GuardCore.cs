using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class GuardCore : AbstractCoreCard
{
    protected override string CoreName => "Guard Core";
    protected override CardModel CanonicalCore => ModelDb.Card<GuardCore>();
    protected override string CoreCycleText => "Gain !Block! Block.";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(2m, ValueProp.Unpowered)];

    public GuardCore() : base(CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCoreEffect(choiceContext);
    }

    protected override async Task OnCoreCycle(PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
    }
}
