using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Zapper : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ZapperPower>(3m)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ZapperPower>(DynamicVars["ZapperPower"].IntValue)];

    public Zapper() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Zapper",
        "#Whenever you gain *Strength* or *Dexterity*, deal {ZapperPower} damage to a random enemy.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ZapperPower>(choiceContext, Owner.Creature,
            DynamicVars["ZapperPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ZapperPower"].UpgradeValueBy(1m);
    }
}
