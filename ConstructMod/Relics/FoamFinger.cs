using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// Draw 1 additional card at the start of your turn for the first 3 turns of each combat.
/// Uses the vanilla ModifyHandDraw hook (same as Bag of Preparation). The on-icon counter
/// is derived statelessly from the round number (3 remaining on round 1, 2 on round 2,
/// 1 on round 3, none after).
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class FoamFinger : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    private const int BonusTurns = 3;
    private const int ExtraCards = 1;

    public override bool ShowCounter =>
        Owner?.Creature.CombatState != null
        && Owner.Creature.CombatState.RoundNumber <= BonusTurns;

    public override int DisplayAmount =>
        Owner?.Creature.CombatState is { } combat
            ? System.Math.Max(0, BonusTurns + 1 - combat.RoundNumber)
            : 0;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner) return count;
        if (player.Creature.CombatState is not { } combat) return count;
        if (combat.RoundNumber > BonusTurns) return count;
        Flash();
        return count + ExtraCards;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Foam Finger",
        Description: "#Draw 1 additional card at the start of your turn for the first 3 turns of each combat.",
        Flavor: "We're number one!");
}
