using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class SiegeFormPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Siege Form",
        Description: "After you play a card, gain {Amount} Strength until the end of your turn.",
        SmartDescription: "After you play a card, gain {Amount} Strength until the end of your turn.");

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return;
        Flash();
        await PowerCmd.Apply<SiegeFormStrengthPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
