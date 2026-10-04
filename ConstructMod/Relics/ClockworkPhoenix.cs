using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Relics;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// A flag relic (no hooks): while owned, it widens the Construct cards' upgrade ceiling so the
/// player can CHOOSE a Mega-upgrade at the smith (out of combat only) — the gate lives in
/// AbstractConstructCard.MaxUpgradeLevel. Faithful to the original, it is never randomly
/// obtainable: its only sources are the fully-upgraded ClockworkEgg (a future port) and,
/// eventually, a character-select config toggle (the original's phoenixStart option).
/// Event rarity is never drawn by the reward RNG despite pool membership.
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class ClockworkPhoenix : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Clockwork Phoenix",
        Description: "#Your upgraded Construct cards can now be *Mega-upgraded* outside of combat.",
        Flavor: "Reborn in cogs and copper.");
}
