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

public class Autoturret : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Turret", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var tip in base.ExtraHoverTips) yield return tip;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
        }
    }

    public Autoturret() : base(1, CardType.Power, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Autoturret",
        "#Whenever a card *Cycles*, deal {Turret} damage to a random enemy.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AutoturretPower>(choiceContext, Owner.Creature,
            DynamicVars["Turret"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Regular: +1 damage per Cycle (1 → 2). Mega: +2 more (2 → 4;
        // StS1 M_UPGRADE_POWER_DAMAGE).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Turret"].UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Turret"].UpgradeValueBy(2m);
        }
    }
}
