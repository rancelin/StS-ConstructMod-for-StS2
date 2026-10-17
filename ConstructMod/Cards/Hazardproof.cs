using System.Collections.Generic;
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

public class Hazardproof : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<ArtifactPower>(null)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(2m, ValueProp.Move),
        new DynamicVar("Artifact", 1m)
    ];

    public Hazardproof() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Hazardproof",
        "#Gain !Block! *Block*.\nGain {Artifact} *Artifact*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature,
            DynamicVars["Artifact"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Regular: +3 Block (2 → 5). Mega: +2 Artifact (1 → 3;
        // StS1 M_UPGRADE_PLUS_ARTIFACT_AMT).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Block.UpgradeValueBy(3m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Artifact"].UpgradeValueBy(2m);
        }
    }
}
