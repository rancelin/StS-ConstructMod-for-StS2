using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class ShieldGenerator : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BlurPower>(null),
        HoverTipFactory.FromPower<VulnerablePower>(null)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Blur", 4m),
        new DynamicVar("Vulnerable", 2m)
    ];

    public ShieldGenerator() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Shield Generator",
        "#Gain {Blur} *Blur*.\nGain {Vulnerable} *Vulnerable*.{IfUpgraded:show:|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BlurPower>(choiceContext, Owner.Creature,
            DynamicVars["Blur"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature,
            DynamicVars["Vulnerable"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: 1 less Vulnerable on yourself (2 → 1). Mega: +2 Blur
        // (StS1 M_UPGRADE_PLUS_BLUR_AMT).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Vulnerable"].UpgradeValueBy(-1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Blur"].UpgradeValueBy(2m);
        }
    }
}
