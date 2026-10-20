using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// At the end of your turn, Exhaust a random Status card in your hand to gain
/// <see cref="MegaCrit.Sts2.Core.Models.PowerModel.Amount"/> Block. Applied by the
/// <see cref="Cards.SunScreen"/> card. Distinct power class from
/// <see cref="SunScreenMegaPower"/> (StS1 reused the same power ID for both; StS2 merges
/// same-ID powers, so the Curse variant needs its own class).
/// </summary>
public class SunScreenPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Sun Screen",
        Description: "#At the end of your turn, Exhaust a random Status card in your hand to gain {Amount} Block.",
        SmartDescription: "#At the end of your turn, Exhaust a random Status card in your hand to gain {Amount} Block.");

    public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        // BeforeSideTurnEndEarly runs BEFORE end-of-turn-in-hand card triggers (Burn's damage)
        // and before the hand flush — so the Status is exhausted before it can hurt you.
        // (The previous BeforeFlush slot ran after the Burn had already triggered.)
        if (side != CombatSide.Player || !participants.Contains(Owner)) return;
        await ExhaustRandomCardType(choiceContext, CardType.Status);
    }

    protected async Task ExhaustRandomCardType(PlayerChoiceContext choiceContext, CardType cardType)
    {
        if (Owner.Player == null) return;
        var candidates = PileType.Hand.GetPile(Owner.Player).Cards
            .Where(c => c.Type == cardType).ToList();
        if (candidates.Count == 0) return;
        var card = Owner.Player.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (card == null) return;
        Flash();
        await CardCmd.Exhaust(choiceContext, card);
        // Block is deliberately NOT boosted by Dexterity (per design decision; matches the
        // Downfall convention for power-granted end-of-turn block like Metallicize).
        await CreatureCmd.GainBlock(Owner, (int)Amount, BlockProps.nonCardUnpowered, null);
    }
}
