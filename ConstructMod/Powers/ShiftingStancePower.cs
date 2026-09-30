using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class ShiftingStancePower : CustomPowerModel
{
    private decimal _remaining;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Shifting Stance",
        Description: "After you play {CardsLeft} more card(s), swap your Strength and Dexterity.",
        SmartDescription: "After you play {CardsLeft} more card(s), swap your Strength and Dexterity.");

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this) _remaining = Amount;
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return Task.CompletedTask;
        _remaining--;
        if (_remaining > 0) return Task.CompletedTask;
        _remaining = Amount;
        Flash();
        return SwapStats(choiceContext);
    }

    private async Task SwapStats(PlayerChoiceContext choiceContext)
    {
        var str = Owner.GetPower<StrengthPower>()?.Amount ?? 0m;
        var dex = Owner.GetPower<DexterityPower>()?.Amount ?? 0m;
        if (dex - str != 0m)
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, dex - str, Owner, null);
        if (str - dex != 0m)
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, str - dex, Owner, null);
    }
}
