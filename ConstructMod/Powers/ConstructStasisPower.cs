using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// The Stasis bank (StS1 ConstructStasisPower): holds one banked card plus the copy count
/// the next Stasis play will reclaim. Reclaim itself lives in the Stasis card's OnPlay
/// (StS1 drives everything through StasisAction on each play): the held card is
/// force-mega-upgraded and returned N times, then a new card is banked.
/// </summary>
public class ConstructStasisPower : CustomPowerModel
{
    private CardModel? _heldCard;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("Card", "")];

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Self-Stasis",
        Description: "#Holding: {Card}. When *Stasis* is played again, add {Amount} *Mega-upgraded* copy/copies to your hand.",
        SmartDescription: "#Holding: {Card}. When *Stasis* is played again, add {Amount} *Mega-upgraded* copy/copies to your hand.");

    public void SetHeldCard(CardModel card)
    {
        _heldCard = card;
        if (DynamicVars.ContainsKey("Card"))
        {
            ((StringVar)DynamicVars["Card"]).StringValue = card.Title;
        }
    }

    public CardModel? HeldCard => _heldCard;
}
