using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// Granted by <see cref="Powers.ExtraLongRangeLancePower"/> on victory. Does NOT fire this
/// combat: at the start of the next combat it replaces itself with a
/// <see cref="LongRangeLanceRelic"/> carrying the same damage counter, so the damage lands at
/// the start of the combat AFTER next. Event rarity is never drawn by the reward RNG, so
/// despite pool membership it's only obtainable via the power (StS1 SPECIAL semantics).
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class ExtraLongRangeLanceRelic : CustomRelicModel
{
    // StS1 RelicTier.SPECIAL — maps to Event: never randomly generated (no [Pool]).
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Damage", 12m)];

    public override bool ShowCounter => true;
    public override int DisplayAmount => DynamicVars["Damage"].IntValue;

    /// <summary>Set the lance's damage counter (called by ExtraLongRangeLancePower on victory).</summary>
    public void SetLanceDamage(int damage)
    {
        DynamicVars["Damage"].BaseValue = damage;
        InvokeDisplayAmountChanged();
    }

    public override Task BeforeCombatStart()
    {
        Flash();
        // Replace this relic with a LongRangeLanceRelic carrying the same damage counter.
        // The replacement does not fire this combat (its BeforeCombatStart has effectively
        // passed — it was just created), so the damage lands next combat instead.
        var lance = ModelDb.Relic<LongRangeLanceRelic>().ToMutable();
        if (lance is LongRangeLanceRelic typed)
        {
            typed.SetLanceDamage(DynamicVars["Damage"].IntValue);
        }
        RelicCmd.Replace(this, lance);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "EXTRA-Long-Range Lance",
        Description: "#Does nothing this combat. At the start of your next combat, this becomes a Long-Range Lance that will deal {Damage} damage to a random enemy.",
        Flavor: "Pointed firmly forwards, and then some.");
}
