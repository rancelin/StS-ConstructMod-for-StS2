using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// When you defeat an Elite, Upgrade a random upgradable card in your deck. Pulses while
/// you're on an Elite room (hinting the pending upgrade).
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class RocketBooster : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task AfterCombatVictory(CombatRoom room)
    {
        if (room.RoomType != RoomType.Elite) return Task.CompletedTask;
        var upgradable = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c.IsUpgradable).ToList();
        if (upgradable.Count == 0) return Task.CompletedTask;
        Flash();
        var card = upgradable[Owner.RunState.Rng.CombatCardSelection.NextInt(0, upgradable.Count)];
        // CardCmd.Upgrade handles the deck-pile bookkeeping + upgrade VFX.
        CardCmd.Upgrade(card);
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        // Pulse while standing on an Elite room (StS1 onEnterRoom pulse hint).
        Status = room.RoomType == RoomType.Elite
            ? RelicStatus.Active
            : RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Rocket Booster",
        Description: "#When you defeat an Elite, Upgrade a random card in your deck.",
        Flavor: "3... 2... 1...");
}
