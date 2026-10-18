using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;
using ConstructMod.Relics;

namespace ConstructMod.Cards;

public class LongRangeLance : AbstractConstructCard
{
    // 2-tier ladder: +1 = +4 damage (12 → 16), +2 = mega (−4 back to 12, EXTRA lance).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Damage", 12m)];

    public LongRangeLance() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Long-Range Lance",
        "#At the start of your next combat, deal !Damage! damage to a random enemy.{IfMega:mega:\nAt the start of the combat after, deal !Damage! damage to a random enemy.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Visual buff (in-combat feedback that a lance is pending).
        await PowerCmd.Apply<LongRangeLancePower>(choiceContext, Owner.Creature,
            DynamicVars["Damage"].IntValue, Owner.Creature, this);

        // Grant the lance relic directly. It stays inert for the rest of this combat
        // (BeforeCombatStart has already passed) and fires at the start of the next combat,
        // dealing the damage and consuming itself — the StS1 outcome. (StS1 spawned the
        // relic at victory via the power's onVictory, but StS2's AfterCombatVictory hook
        // never reaches powers — the run-level dispatch only visits relics/potions/cards —
        // so the card grants the relic itself.) Repeat plays in the same combat add to
        // the held relic (StS1 stacks the power into one summed relic), never duplicates.
        var held = Owner.Relics?.OfType<LongRangeLanceRelic>().FirstOrDefault();
        if (held != null)
        {
            held.AddLanceDamage(DynamicVars["Damage"].IntValue);
        }
        else
        {
            var lance = await RelicCmd.Obtain<LongRangeLanceRelic>(Owner);
            lance?.SetLanceDamage(DynamicVars["Damage"].IntValue);
        }

        // Mega tier: also grant the EXTRA lance, which skips the next combat
        // and replaces itself with a Lance relic, so the second hit lands the combat after.
        if (IsMegaUpgraded)
        {
            await PowerCmd.Apply<ExtraLongRangeLancePower>(choiceContext, Owner.Creature,
                DynamicVars["Damage"].IntValue, Owner.Creature, this);
            var heldExtra = Owner.Relics?.OfType<ExtraLongRangeLanceRelic>().FirstOrDefault();
            if (heldExtra != null)
            {
                heldExtra.AddLanceDamage(DynamicVars["Damage"].IntValue);
            }
            else
            {
                var extra = await RelicCmd.Obtain<ExtraLongRangeLanceRelic>(Owner);
                extra?.SetLanceDamage(DynamicVars["Damage"].IntValue);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +4 damage (12 → 16). Mega: −4 (16 → 12, net 0); the EXTRA power
        // moves here (OnPlay, gated on IsMegaUpgraded).
        DynamicVars["Damage"].UpgradeValueBy(CurrentUpgradeLevel == 1 ? 4m : -4m);
    }
}
