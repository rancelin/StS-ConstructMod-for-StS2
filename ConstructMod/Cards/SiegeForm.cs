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

public class SiegeForm : AbstractConstructCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>(null)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SiegeFormPower>(2m)
    ];

    public SiegeForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Siege Form",
        "#After you play a card, gain {SiegeFormPower} *Strength* until the end of your turn.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SiegeFormPower>(choiceContext, Owner.Creature,
            DynamicVars["SiegeFormPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SiegeFormPower"].UpgradeValueBy(1m);
    }
}
