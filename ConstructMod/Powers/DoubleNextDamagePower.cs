using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// StS1's <c>DoubleNextDamagePower</c>: doubles the damage of the owner's next *powered* attack(s),
/// then is consumed (one charge spent per attack card played). Expires at the end of the round.
/// Used by the un-upgraded Isolate ("Your next attack deals double damage this turn."). The
/// upgraded/mega tiers use vanilla <see cref="MegaCrit.Sts2.Core.Models.Powers.DoubleDamagePower"/>
/// instead (all attacks, 1/2 turns).
/// </summary>
public class DoubleNextDamagePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Double Next Damage",
        Description: "#Your next attack deals double damage.",
        SmartDescription: "#Your next {Amount} attack(s) deal double damage.");

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer == null) return 1m;
        if (dealer != Owner && !Owner.Pets.Contains<Creature>(dealer)) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        if (cardSource == null) return 1m;
        return 2m;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;
        // Only spend a charge on the first play of a series (Replay etc. shouldn't eat multiple).
        if (!cardPlay.IsFirstInSeries) return;
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}
