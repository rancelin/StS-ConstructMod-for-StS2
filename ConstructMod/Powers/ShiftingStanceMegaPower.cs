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

/// <summary>
/// The Shifting Stance mega tier (StS1 ShiftingStanceMegaPower): same countdown swap as
/// the base power, but after swapping it increases the highest of Strength/Dexterity
/// and decreases the lowest by 1 (StS1 IncreaseDecreaseStatsAction semantics; ties do
/// nothing). Mirrors ShiftingStancePower structurally (including its single-instance
/// Counter shape — StS1's unique-ID non-stacking is a known follow-up, same as base).
/// </summary>
public class ShiftingStanceMegaPower : CustomPowerModel
{
    private decimal _remaining;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("CardsLeft", 0m)];

    private void SyncCardsLeft() => DynamicVars["CardsLeft"].BaseValue = _remaining;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Shifting Stance (Mega)",
        Description: "#After you play {CardsLeft} more card(s), swap your Strength and Dexterity, then increase the highest and decrease the lowest by 1.",
        SmartDescription: "#After you play {CardsLeft} more card(s), swap your Strength and Dexterity, then increase the highest and decrease the lowest by 1.");

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
        _remaining--;
        if (_remaining > 0)
        {
            SyncCardsLeft();
            return Task.CompletedTask;
        }
        _remaining = Amount;
        SyncCardsLeft();
        Flash();
        return SwapAndAdjustStats(choiceContext);
    }

    private async Task SwapAndAdjustStats(PlayerChoiceContext choiceContext)
    {
        var str = Owner.GetPower<StrengthPower>()?.Amount ?? 0m;
        var dex = Owner.GetPower<DexterityPower>()?.Amount ?? 0m;
        if (dex - str != 0m)
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, dex - str, Owner, null);
        if (str - dex != 0m)
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, str - dex, Owner, null);
        // Post-swap adjustment (StS1 IncreaseDecreaseStatsAction): +1 highest, -1 lowest.
        str = Owner.GetPower<StrengthPower>()?.Amount ?? 0m;
        dex = Owner.GetPower<DexterityPower>()?.Amount ?? 0m;
        if (dex > str)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 1m, Owner, null);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -1m, Owner, null);
        }
        else if (str > dex)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1m, Owner, null);
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, -1m, Owner, null);
        }
    }
}
