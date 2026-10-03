using System.Collections.Generic;
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
    // Damage = CalculationBase (0) + ExtraDamage (Multiplier) * (cycles this turn).
    // Uses the vanilla CalculatedDamageVar pattern (same as PerfectedStrike) so the damage
    // flows through the card's damage pipeline (Strength/Weak/Vulnerable/etc. apply correctly)
    // and the card preview shows the computed value. The previous DamageCmd.Attack(decimal)
    // call bypassed the pipeline and hung when actually dealing damage.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new CalculationBaseVar(0m),
            new ExtraDamageVar(3m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
                CycleCount.GetCyclesThisTurn(card.Owner))
        ];

    public Rollout() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        Overheat = 10;
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
        "#Deal damage equal to {ExtraDamage} times the number of cards that have *Cycled* this turn.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        var dmg = DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        if (dmg <= 0) return;
        // DamageCmd.Attack(CalculatedDamageVar) does not auto-target (unlike the decimal overload),
        // so .Targeting(cardPlay.Target) is required — matches vanilla PerfectedStrike.
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_giant_horizontal_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
        // Original: upgradeOverheat(+5) on upgrade, then mega upgradeOverheat(+5).
        // Folded: +10 overheat (combined), so the card tolerates 10 more cycles before Burning.
        UpgradeOverheat(10);
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
