using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;
using ConstructMod.Powers;

namespace ConstructMod.Relics;

/// <summary>
/// Your cards cannot Overheat for the first 3 turns of each combat (applies
/// <see cref="FlashFreezePower"/> 3 at combat start).
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class IceCubes : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Turns", 3m)];

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.IsDead) return;
        Flash();
        await PowerCmd.Apply<FlashFreezePower>(new ThrowingPlayerChoiceContext(),
            Owner.Creature, DynamicVars["Turns"].BaseValue, Owner.Creature, null);
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Ice Cubes",
        Description: "#Your cards cannot *Overheat* for the first {Turns} turns of each combat.",
        Flavor: "A bag of eternally-cold machine coolant.");
}
