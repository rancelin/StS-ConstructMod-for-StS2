using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;
using ConstructMod.Hooks;

namespace ConstructMod.Powers;

/// <summary>
/// Whenever a card Overheats (turns into a Burn from cycling), gain <see cref="PowerModel.Amount"/>
/// Strength and <see cref="PowerModel.Amount"/> Dexterity. Implements <see cref="IAfterCardOverheated"/>.
/// </summary>
public class AgitationPower : CustomPowerModel, IAfterCardOverheated
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Agitation",
        Description: "Whenever a card Overheats, gain {Amount} Strength and {Amount} Dexterity.",
        SmartDescription: "Whenever a card Overheats, gain {Amount} Strength and {Amount} Dexterity.");

    public async Task AfterCardOverheated(PlayerChoiceContext ctx, CardModel overheatedCard)
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(ctx, Owner, Amount, Owner, null);
        await PowerCmd.Apply<DexterityPower>(ctx, Owner, Amount, Owner, null);
    }
}
