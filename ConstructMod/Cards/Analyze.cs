using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Analyze : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [EnergyHoverTip];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new DynamicVar("Draw", 2)
    ];

    public Analyze() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Analyze",
        "#Next turn, gain {Energy:energyIcons()} and draw {Draw} more cards.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature,
            DynamicVars.Energy.IntValue, Owner.Creature, this);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature,
            DynamicVars["Draw"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +1 card drawn (2 → 3). Mega: +1 energy AND +1 card
        // (StS1 M_UPGRADE_PLUS_ENERGIZE_AMT / M_UPGRADE_PLUS_DRAW_AMT).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Draw"].UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Energy.UpgradeValueBy(1m);
            DynamicVars["Draw"].UpgradeValueBy(1m);
        }
    }
}
