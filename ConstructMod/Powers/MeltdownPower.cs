using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class MeltdownPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Meltdown",
        Description: "At the start of your turn, deal {Amount} damage to ALL enemies and add a Burn to your hand.",
        SmartDescription: "At the start of your turn, deal {Amount} damage to ALL enemies and add a Burn to your hand.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState is not { } combat) return;
        foreach (var enemy in combat.HittableEnemies.ToList())
        {
            await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner);
        }
        var burn = combat.CreateCard(ModelDb.Card<Burn>(), player);
        await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Hand, player);
    }
}
