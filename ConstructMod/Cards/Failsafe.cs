using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Failsafe : AbstractConstructCard
{
    // 2-tier ladder: +1 = +1 card (2 → 3), +2 = mega (+4 cards → 7).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("FailsafePower", 2m)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(ConstructKeywords.Cycle)];

    public Failsafe() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Failsafe",
        "#The first !FailsafePower! Status cards you draw each turn *Cycle*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FailsafePower>(choiceContext, Owner.Creature,
            DynamicVars["FailsafePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +1 (2 → 3). Mega: +4 (3 → 7).
        DynamicVars["FailsafePower"].UpgradeValueBy(CurrentUpgradeLevel == 1 ? 1m : 4m);
    }
}
