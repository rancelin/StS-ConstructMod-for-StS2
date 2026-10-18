using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Enhance : AbstractConstructCard
{
    // 2-tier ladder: +1 = one more card, +2 = mega (force-mega instead of regular upgrade).
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Enhance", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            // The card only mentions Mega-upgrading in its mega text line, so the tip appears
            // exactly when that line does (mega tier, or the +1→+2 preview).
            if (IsMegaUpgraded) yield return HoverTipFactory.FromKeyword(ConstructKeywords.MegaUpgrade);
        }
    }

    public Enhance() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Enhance",
        "#At the end of your turn, Upgrade !Enhance! random card(s) in your discard pile for the rest of this combat.{IfMega:mega:\nCan *Mega-upgrade* cards.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsMegaUpgraded)
        {
            await PowerCmd.Apply<EnhanceMegaPower>(choiceContext, Owner.Creature,
                DynamicVars["Enhance"].BaseValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<EnhancePower>(choiceContext, Owner.Creature,
                DynamicVars["Enhance"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: +1 card (1 → 2).
            DynamicVars["Enhance"].UpgradeValueBy(1m);
        }
        // Mega: no stat change — the tier swaps the power (OnPlay, gated on IsMegaUpgraded).
    }
}
