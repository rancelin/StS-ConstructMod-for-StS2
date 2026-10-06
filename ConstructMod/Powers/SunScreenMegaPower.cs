using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

/// <summary>
/// Mega variant of <see cref="SunScreenPower"/> (applied by the upgraded SunScreen card, which
/// keeps the regular power too): at the end of your turn, Exhaust a random Curse card in your
/// hand to gain <see cref="MegaCrit.Sts2.Core.Models.PowerModel.Amount"/> Block.
/// </summary>
public class SunScreenMegaPower : SunScreenPower
{
    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Sun Screen (Mega)",
        Description: "At the end of your turn, Exhaust a random Curse card in your hand to gain {Amount} Block.",
        SmartDescription: "At the end of your turn, Exhaust a random Curse card in your hand to gain {Amount} Block.");

    public override Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return Task.CompletedTask;
        return ExhaustRandomCardType(choiceContext, CardType.Curse);
    }
}
