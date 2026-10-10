using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace ConstructMod.Pools;

public class ConstructRelicPool : CustomRelicPoolModel
{
    public override string? BigEnergyIconPath => ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/energy_ironclad.tres");
    public override string? TextEnergyIconPath => ImageHelper.GetImagePath("packed/sprite_fonts/ironclad_energy_icon.png");
}
