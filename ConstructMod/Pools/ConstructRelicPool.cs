using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using BaseLib.Abstracts;
using ConstructMod.Relics;

namespace ConstructMod.Pools;

public class ConstructRelicPool : CustomRelicPoolModel
{
    public override string EnergyColorName => ConstructCardPool.TheConstruct_EnergyColor;

    protected override IEnumerable<RelicModel> GenerateAllRelics() =>
    [
        ModelDb.Relic<Cogwheel>()
    ];
}
