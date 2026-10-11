using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Zapper : AbstractConstructCard
{
    // 2-tier ladder: +1 = cost 2 → 1, +2 = mega (trigger on Strength/Dexterity loss too).
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ZapperPower>(3m)];
    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(null),
        HoverTipFactory.FromPower<DexterityPower>(null),
        HoverTipFactory.FromKeyword(ConstructKeywords.MegaUpgrade)
    ];

    public Zapper() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Zapper",
        "#Whenever you gain{IfMega:mega: or *lose*|} *Strength* or *Dexterity*, deal {ZapperPower} damage to a random enemy. It loses 1 *Strength* this turn.");
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ZapperPower>(choiceContext, Owner.Creature,
            DynamicVars["ZapperPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: cost 2 → 1. (The loss-trigger is the mega tier, gated in
            // ZapperPower via the mega-upgraded card source.)
            EnergyCost.UpgradeBy(-1);
        }
    }
}
