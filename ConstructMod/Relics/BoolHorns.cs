using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// At the start of each combat, deal damage equal to the counter to a random enemy
/// (unpowered), then reset the counter to 3. Entering a non-combat room doubles the
/// counter (3 → 6 → 12 → ...), so the damage accumulates between fights.
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class BoolHorns : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    private const int BaseDamage = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Damage", (decimal)BaseDamage)];

    public override bool ShowCounter => true;
    public override int DisplayAmount => DynamicVars["Damage"].IntValue;

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
        // Reset the counter after the combat starts, and stop pulsing.
        DynamicVars["Damage"].BaseValue = BaseDamage;
        Status = RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        // StS1 justEnteredRoom: doubling on entering any NON-combat room.
        if (room.RoomType is not (RoomType.Monster or RoomType.Elite or RoomType.Boss))
        {
            DynamicVars["Damage"].BaseValue *= 2;
            Flash();
            Status = RelicStatus.Active; // pulse while the damage is doubled
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Bool Horns",
        Description: "#At the start of each combat, deal {Damage} damage to a random enemy. Entering a non-combat room doubles this.",
        Flavor: "1 + 1 = true.");
}
