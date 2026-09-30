using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Enhance : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<EnhancePower>("Enhance", 1m)];

    public Enhance() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Enhance",
        "#At the end of your turn, Upgrade {Enhance} random card(s) in your discard pile for the rest of this combat.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EnhancePower>(choiceContext, Owner.Creature,
            DynamicVars["Enhance"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Enhance"].UpgradeValueBy(1m);
    }
}
