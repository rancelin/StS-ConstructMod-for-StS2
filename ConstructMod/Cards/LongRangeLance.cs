using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class LongRangeLance : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Damage", 12m)];

    public LongRangeLance() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Long-Range Lance",
        "#At the start of your next combat, deal {Damage} damage to a random enemy.{IfUpgraded:show:\nAt the start of the combat after, deal {Damage} damage to a random enemy.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<LongRangeLancePower>(choiceContext, Owner.Creature,
            DynamicVars["Damage"].IntValue, Owner.Creature, this);
        // Mega folded into upgrade: also apply the EXTRA variant (damage lands 2 combats later).
        // Original: upgrade +4 damage, mega -4 damage (net 0) + this second power.
        if (IsUpgraded)
        {
            await PowerCmd.Apply<ExtraLongRangeLancePower>(choiceContext, Owner.Creature,
                DynamicVars["Damage"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Original: +4 damage on upgrade, then mega -4 (net 0) + the EXTRA-Long-Range power.
        // Folded: no stat change; the upgrade adds the EXTRA power (applied in OnPlay above).
    }
}
