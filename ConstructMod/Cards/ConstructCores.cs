using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Cards;

/// <summary>
/// Shared random-Core picker, used by Create Cores, Panic Fire, and the Master Core relic.
/// The original's core pool (ConstructMod.cores) included Nuclear Core only when the
/// "Overheated" expansion was enabled — the port ships the expansion as always-on, so all
/// six cores are eligible. Uses the run's card-selection RNG for determinism.
/// </summary>
public static class ConstructCores
{
    public static CardModel RandomCore(Player player)
    {
        // ModelDb.Card<T>() returns canonical models (safe to pass to combat.CreateCard).
        return player.RunState.Rng.CombatCardSelection.NextInt(0, 6) switch
        {
            0 => ModelDb.Card<FlameCore>(),
            1 => ModelDb.Card<LaserCore>(),
            2 => ModelDb.Card<ScopeCore>(),
            3 => ModelDb.Card<ForceCore>(),
            4 => ModelDb.Card<GuardCore>(),
            _ => ModelDb.Card<NuclearCore>()
        };
    }
}
