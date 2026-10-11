using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class ShiftingStancePower : CustomPowerModel
{
    private decimal _remaining;
    private CardModel? _sourceCard;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("CardsLeft", 0m)];

    private void SyncCardsLeft() => DynamicVars["CardsLeft"].BaseValue = _remaining;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Shifting Stance",
        Description: "#After you play {CardsLeft} more card(s), swap your Strength and Dexterity.",
        SmartDescription: "#After you play {CardsLeft} more card(s), swap your Strength and Dexterity.");

    public override Task BeforeApplied(Creature target, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        // StS1 applies the power addToBottom (after the play resolves), so the creating
        // play never counts itself (SpinDrive/SiegeForm pattern). First apply only —
        // later ShiftingStance copies correctly count, as in StS1.
        if (cardSource != null && _sourceCard == null) _sourceCard = cardSource;
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            _remaining = Amount;
            SyncCardsLeft();
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return Task.CompletedTask;
        if (ReferenceEquals(cardPlay.Card, _sourceCard)) return Task.CompletedTask;
        _remaining--;
        if (_remaining > 0)
        {
            SyncCardsLeft();
            return Task.CompletedTask;
        }
        _remaining = Amount;
        SyncCardsLeft();
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
