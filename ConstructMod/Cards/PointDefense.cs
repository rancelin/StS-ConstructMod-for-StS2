using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class PointDefense : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Limit", 5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var tip in base.ExtraHoverTips) yield return tip;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public PointDefense() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Point Defense",
        "#Whenever a card *Cycles*, gain 1 *Block*. Works !Limit! times per turn.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PointDefensePower>(choiceContext, Owner.Creature,
            DynamicVars["Limit"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Regular: +5 uses per turn (5 → 10). Mega: +89 more (10 → 99; StS1
        // M_UPGRADE_PLUS_POWER_BLOCK_LIMIT — effectively unlimited).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Limit"].UpgradeValueBy(5m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Limit"].UpgradeValueBy(89m);
        }
    }
}
