using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class ReactiveShield : AbstractConstructCard
{
    public override bool GainsBlock => true;

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ReactiveShieldPower>(3m)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public ReactiveShield() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Reactive Shield",
        "#Whenever you gain *Block*, deal {ReactiveShieldPower} damage to the lowest-HP enemy.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ReactiveShieldPower>(choiceContext, Owner.Creature,
            DynamicVars["ReactiveShieldPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ReactiveShieldPower"].UpgradeValueBy(1m);
    }
}
