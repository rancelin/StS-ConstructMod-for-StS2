using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Pools;

public class ConstructCardPool : CustomCardPoolModel
{
    public override string Title => "construct";
    public override string EnergyColorName => TheConstruct_EnergyColor;
    public const string TheConstruct_EnergyColor = "construct";
    public override string CardFrameMaterialPath => "card_frame_red";
    public override Color DeckEntryCardColor => new Color("AA9632");
    public override Color EnergyOutlineColor => new Color("5C5440");
    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards() =>
    [
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<ModeShift>()
    ];
}
