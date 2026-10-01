using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Cards;

public class Rollout : AbstractCycleCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Multiplier", 3m), new DamageVar(0m, ValueProp.Move)];

    public Rollout() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Rollout",
        "#Deal damage equal to {Multiplier} times the number of cards that have *Cycled* this turn. NL *Cycle*.");

    public override bool CanCycle() => base.CanCycle() && PileType.Hand.GetPile(Owner).Cards.Any(c => c != this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cycles = CycleCount.GetCyclesThisTurn(Owner);
        var damage = cycles * DynamicVars["Multiplier"].IntValue;
        if (damage <= 0) return;
        await DamageCmd.Attack(damage).FromCard(this, cardPlay)
            .WithHitFx("vfx/vfx_giant_horizontal_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1m);
    }
}

public static class CycleCount
{
    private static readonly System.Collections.Generic.Dictionary<Player, int> _cycles = new();

    public static int GetCyclesThisTurn(Player player) =>
        _cycles.TryGetValue(player, out var n) ? n : 0;

    public static void Increment(Player player)
    {
        _cycles[player] = GetCyclesThisTurn(player) + 1;
    }

    public static void ResetTurn(Player player)
    {
        _cycles.Remove(player);
    }
}
