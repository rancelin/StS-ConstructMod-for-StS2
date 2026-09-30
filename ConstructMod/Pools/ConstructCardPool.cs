using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using BaseLib.Abstracts;

namespace ConstructMod.Pools;

public class ConstructCardPool : CustomCardPoolModel
{
    public override string Title => "construct";
    public override string CardFrameMaterialPath => "card_frame_red";
    public override Color DeckEntryCardColor => new Color("AA9632");
    public override Color EnergyOutlineColor => new Color("5C5440");
    public override bool IsColorless => false;

    // Placeholder energy icons: reuse the red (Ironclad) art until custom art ships.
    // Big: card-cost atlas sprite. Text: small sprite-font icon (vanilla text icon path format).
    public override string? BigEnergyIconPath => ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/energy_ironclad.tres");
    public override string? TextEnergyIconPath => ImageHelper.GetImagePath("packed/sprite_fonts/ironclad_energy_icon.png");
}
