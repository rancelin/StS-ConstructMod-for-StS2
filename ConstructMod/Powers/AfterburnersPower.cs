using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class AfterburnersPower : CustomPowerModel
{
    private CardModel? _sourceCard;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Afterburners",
        Description: "#This turn, your next {Amount} non-Rare card(s) is/are played twice.",
        SmartDescription: "#This turn, your next {Amount} non-Rare card(s) is/are played twice.");

    public override Task BeforeApplied(Creature target, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (cardSource != null && _sourceCard == null) _sourceCard = cardSource;
        return Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner || Amount <= 0) return playCount;
        if (ReferenceEquals(card, _sourceCard)) return playCount;
        if (card.Rarity == CardRarity.Rare) return playCount;
        SetAmount(Amount - 1);
        return playCount + 1;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        // Charges spent: remove immediately so the icon/tooltip doesn't linger at 0
        // until turn end (behavior was already correct — only the display lagged).
        if (Amount <= 0) await PowerCmd.Remove(this);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}
