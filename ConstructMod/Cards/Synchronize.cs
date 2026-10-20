using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Synchronize : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    // Innate is granted by the +1 rung (and kept at +2), so the tip belongs only on an upgraded
    // card — including the +1→+2 preview, whose clone already carries the keyword.
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            if (IsUpgraded) yield return HoverTipFactory.FromKeyword(CardKeyword.Innate);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SynchronizePower>(8m)
    ];

    public Synchronize() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Synchronize",
        "#When you draw 2 of the same card in a row, deal !SynchronizePower! damage to ALL enemies.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SynchronizePower>(choiceContext, Owner.Creature,
            DynamicVars["SynchronizePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: Innate only. Mega: +4 damage per matched pair
        // (StS1 M_UPGRADE_PLUS_POWER_DAMAGE).
        if (CurrentUpgradeLevel == 1)
        {
            AddKeyword(CardKeyword.Innate);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["SynchronizePower"].UpgradeValueBy(4m);
        }
    }
}
