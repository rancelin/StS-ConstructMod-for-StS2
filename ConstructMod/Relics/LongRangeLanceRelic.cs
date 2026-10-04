using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// Granted by <see cref="Powers.LongRangeLancePower"/> on victory. At the start of the next
/// combat, deals <see cref="DynamicVars"/>[Damage] damage to a random enemy (unpowered — not
/// boosted by Strength), then consumes itself. Event rarity is never drawn by the reward RNG,
/// so despite pool membership it's only obtainable via the power (StS1 SPECIAL semantics).
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class LongRangeLanceRelic : CustomRelicModel
{
    // StS1 RelicTier.SPECIAL — maps to Event: never randomly generated (no [Pool]).
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Damage", 12m)];

    public override bool ShowCounter => true;
    public override int DisplayAmount => DynamicVars["Damage"].IntValue;

    /// <summary>Set the lance's damage counter (called by LongRangeLancePower on victory).</summary>
    public void SetLanceDamage(int damage)
    {
        DynamicVars["Damage"].BaseValue = damage;
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is not { } combat) return;
        var enemy = Owner.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies);
        Flash();
        if (enemy != null)
        {
            // THORNS-equivalent damage: unpowered, not boosted by Strength.
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                enemy, DynamicVars["Damage"].IntValue, ValueProp.Unpowered, Owner.Creature);
        }
        // Consume the relic (StS1 LoseRelicAction).
        await RelicCmd.Remove(this);
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Long-Range Lance",
        Description: "#At the start of this combat, deal {Damage} damage to a random enemy. Vanishes afterwards.",
        Flavor: "Pointed firmly forwards.");
}
