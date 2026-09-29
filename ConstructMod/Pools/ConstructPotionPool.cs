using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace ConstructMod.Pools;

public class ConstructPotionPool : CustomPotionPoolModel
{
    public override string EnergyColorName => ConstructCardPool.TheConstruct_EnergyColor;

    protected override IEnumerable<PotionModel> GenerateAllPotions() => [];
}
