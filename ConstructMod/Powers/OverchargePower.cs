using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class OverchargePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Overcharge",
        Description: "At the start of your turn, gain 1 energy and add a Burn to your hand.",
        SmartDescription: "At the start of your turn, gain 1 energy and add a Burn to your hand.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState is not { } combat) return;
        await PlayerCmd.GainEnergy(1m, player);
        var burn = combat.CreateCard(ModelDb.Card<Burn>(), player);
        await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Hand, player);
    }
}
