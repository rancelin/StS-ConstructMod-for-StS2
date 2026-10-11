using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class ShiftingStance : AbstractConstructCard
{
    // 2-tier ladder: +1 = fewer cards per swap (3 → 2), +2 = mega (swap + adjust).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(null),
        HoverTipFactory.FromPower<DexterityPower>(null),
        HoverTipFactory.FromKeyword(ConstructKeywords.MegaUpgrade)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShiftingStancePower>(3m)
    ];

    public ShiftingStance() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Shifting Stance",
        "#Every time you play {ShiftingStancePower} cards, swap your *Strength* and *Dexterity*.{IfMega:mega:\nThen increase the highest and decrease the lowest by 1.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsMegaUpgraded)
        {
            await PowerCmd.Apply<ShiftingStanceMegaPower>(choiceContext, Owner.Creature,
                DynamicVars["ShiftingStancePower"].BaseValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<ShiftingStancePower>(choiceContext, Owner.Creature,
                DynamicVars["ShiftingStancePower"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: fewer cards per swap (3 → 2).
            DynamicVars["ShiftingStancePower"].UpgradeValueBy(-1m);
        }
        // Mega: no stat change — the tier swaps the power (OnPlay, gated on IsMegaUpgraded).
    }
}
