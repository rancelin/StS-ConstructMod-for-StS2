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
    // StS1 RelicTier.SPECIAL — maps to Event: never randomly generated.
    // (Pool membership is required for the model to load; Event rarity excludes it from rewards.)
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Damage", 12m)];

    // DynamicVar mutations are NOT saved across save/load — this field is the persisted
    // source of truth for the accumulated counter (BaseLib auto-saves it per relic
    // instance). Synced back into the display var on every path that can run post-load.
    private static readonly SavedSpireField<LongRangeLanceRelic, int> SavedDamage = new(() => 12, "ConstructMod_LongRangeLance_Damage");

    public override bool ShowCounter => true;
    public override int DisplayAmount
    {
        get
        {
            DynamicVars["Damage"].BaseValue = SavedDamage.Get(this);
            return DynamicVars["Damage"].IntValue;
        }
    }

    /// <summary>Set the lance's damage counter (called by LongRangeLancePower on victory).</summary>
    public void SetLanceDamage(int damage)
    {
        DynamicVars["Damage"].BaseValue = damage;
        SavedDamage.Set(this, damage);
        InvokeDisplayAmountChanged();
    }

    /// <summary>Add to the counter for additional Lance plays in the same combat
    /// (StS1 stacks the power pre-victory into a single summed relic).</summary>
    public void AddLanceDamage(int damage) => SetLanceDamage(DynamicVars["Damage"].IntValue + damage);

    public override async Task BeforeCombatStart()
    {
        // Restore the persisted counter (post-save/load the DynamicVar is back at base).
        DynamicVars["Damage"].BaseValue = SavedDamage.Get(this);
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
        Description: "#At the start of your next combat, deal {Damage} damage to a random enemy. Vanishes afterwards.",
        Flavor: "Pointed firmly forwards.");
}
