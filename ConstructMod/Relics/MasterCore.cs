using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Cards;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// At the start of each combat, shuffle 3 random Cores into your draw pile.
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class MasterCore : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Cores", 3m)];

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is not { } combat) return;
        Flash();
        for (var i = 0; i < DynamicVars["Cores"].IntValue; i++)
        {
            var core = combat.CreateCard(ConstructCores.RandomCore(Owner), Owner);
            await CardPileCmd.AddGeneratedCardToCombat(core, PileType.Draw, Owner, CardPilePosition.Random);
        }
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Master Core",
        Description: "#At the start of each combat, shuffle {Cores} random Core(s) into your draw pile.",
        Flavor: "The heart of the machine.");
}
