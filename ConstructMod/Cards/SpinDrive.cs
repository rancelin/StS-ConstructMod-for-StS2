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

public class SpinDrive : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<SpinDrivePower>("Draws", 3m)];

    public SpinDrive() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Spin Drive",
        "#Whenever you play a card, draw a card.\nWorks {Draws} times per turn.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SpinDrivePower>(choiceContext, Owner.Creature,
            DynamicVars["Draws"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: cost 3 → 2. Mega: +5 free draws (3 → 8;
        // StS1 M_UPGRADE_PLUS_FREE_DRAWS).
        if (CurrentUpgradeLevel == 1)
        {
            EnergyCost.UpgradeBy(-1);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Draws"].UpgradeValueBy(5m);
        }
    }
}
