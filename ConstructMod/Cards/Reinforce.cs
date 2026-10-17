using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Reinforce : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(3m, ValueProp.Move),
        new RepeatVar(2)
    ];

    public Reinforce() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Reinforce",
        "#Gain !Block! *Block* {Repeat} times.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        // Regular: +1 repetition (2 → 3). Mega: +2 more (3 → 5;
        // StS1 M_UPGRADE_TIMES_APPLIED).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Repeat.UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Repeat.UpgradeValueBy(2m);
        }
    }
}
