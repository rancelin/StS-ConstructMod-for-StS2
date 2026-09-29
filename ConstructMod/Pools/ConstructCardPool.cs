using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using BaseLib.Abstracts;

namespace ConstructMod.Pools;

public class ConstructCardPool : CustomCardPoolModel
{
    public override string Title => "construct";
    public override string EnergyColorName => "construct";
    public override string CardFrameMaterialPath => "card_frame_red";
    public override Color DeckEntryCardColor => new Color("AA9632");
    public override Color EnergyOutlineColor => new Color("5C5440");
    public override bool IsColorless => false;
}
