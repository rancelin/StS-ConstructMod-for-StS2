using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class PointDefense : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<PointDefensePower>("Limit", 5m)];

    public PointDefense() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Point Defense",
        "#Whenever a card *Cycles*, gain 1 *Block*. Works {Limit} times per turn.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PointDefensePower>(choiceContext, Owner.Creature,
            DynamicVars["Limit"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Limit"].UpgradeValueBy(5m);
    }
}
