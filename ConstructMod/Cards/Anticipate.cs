using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Anticipate : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(11m, ValueProp.Move)];

    public Anticipate() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Anticipate",
        "#Next turn, gain !Block! *Block*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, Owner.Creature,
            DynamicVars.Block.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Regular: +3 Block (11 → 14). Mega: +6 more (14 → 20; StS1 M_UPGRADE_BLOCK_AMT).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Block.UpgradeValueBy(3m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Block.UpgradeValueBy(6m);
        }
    }
}
