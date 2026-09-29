using MegaCrit.Sts2.Core.Helpers;
using BaseLib.Abstracts;

namespace ConstructMod.Pools;

public class ConstructPotionPool : CustomPotionPoolModel
{
    public override string? BigEnergyIconPath => ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/energy_red.tres");
    public override string? TextEnergyIconPath => ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/energy_red.tres");
}
