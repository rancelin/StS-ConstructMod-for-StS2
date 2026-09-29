using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

[Pool(typeof(ConstructRelicPool))]
public class Cogwheel : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
