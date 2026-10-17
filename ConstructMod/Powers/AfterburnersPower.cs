using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// StS1 AfterburnersPower: this turn, the next non-Rare card the owner plays is played
/// <see cref="PowerModel.Amount"/> times, then the power is consumed.
/// </summary>
public class AfterburnersPower : CustomPowerModel
{
    private CardModel? _sourceCard;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Afterburners",
        Description: "#This turn, your next non-Rare card is played {Amount} times.",
        SmartDescription: "#This turn, your next non-Rare card is played {Amount} times.");

    public override Task BeforeApplied(Creature target, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (cardSource != null && _sourceCard == null) _sourceCard = cardSource;
        return Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner || Amount <= 1) return playCount;
        if (ReferenceEquals(card, _sourceCard)) return playCount;
        if (card.Rarity == CardRarity.Rare) return playCount;
        // Total plays = Amount (playCount starts at 1); the power is consumed after this play.
        return playCount + (Amount - 1);
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        // Consume the whole power: it only ever affects the single next non-Rare card.
        await PowerCmd.Remove(this);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}
