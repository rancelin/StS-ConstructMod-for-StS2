using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Cards;

/// <summary>
/// Notifies subscribed models (powers, relics) whenever a card Cycles.
/// Subscribers should subscribe in BeforeApplied and unsubscribe in AfterRemoved.
/// </summary>
public static class CycleEvents
{
    public static event Func<PlayerChoiceContext, CardModel, Task>? CardCycled;

    public static async Task NotifyCycle(PlayerChoiceContext choiceContext, CardModel card)
    {
        var handler = CardCycled;
        if (handler == null) return;
        foreach (Func<PlayerChoiceContext, CardModel, Task> d in handler.GetInvocationList().Cast<Func<PlayerChoiceContext, CardModel, Task>>())
        {
            await d(choiceContext, card);
        }
    }
}
