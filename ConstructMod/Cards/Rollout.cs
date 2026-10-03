using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Cards;

namespace ConstructMod.Cards;

public class Rollout : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Multiplier", 3m), new DamageVar(0m, ValueProp.Move)];

    public Rollout() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Rollout",
        "#Deal damage equal to {Multiplier} times the number of cards that have *Cycled* this turn.");


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
    // Per-PlayerCombatState counter (one per player per combat). Backed by a ConditionalWeakTable via BaseLib's
    // SpireField, so it is automatically cleaned up when the combat state is GC'd, multiplayer-safe, and never
    // leaks across combats (unlike a static Dictionary<Player, int>).
    private static readonly BaseLib.Utils.SpireField<MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState, int> _cycles = new(() => 0);

    public static int GetCyclesThisTurn(MegaCrit.Sts2.Core.Entities.Players.Player player)
        => player.PlayerCombatState is { } pcs ? _cycles.Get(pcs) : 0;

    public static void Increment(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player.PlayerCombatState is not { } pcs) return;
        _cycles.Set(pcs, _cycles.Get(pcs) + 1);
    }

    public static void ResetTurn(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player.PlayerCombatState is not { } pcs) return;
        _cycles.Set(pcs, 0);
    }
}
