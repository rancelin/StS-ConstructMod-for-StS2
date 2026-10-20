using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class CriticalHit : AbstractCycleCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(14m, ValueProp.Move)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<WeakPower>(null);
            yield return HoverTipFactory.FromPower<VulnerablePower>(null);
        }
    }

    public CriticalHit() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Critical Hit",
        "#*Cycle* unless an enemy is *Weak* or *Vulnerable*.\nDeal !Damage! damage.");

    // StS1's mega branch renames the card outright (name = EXTENDED_DESCRIPTION[0] = "Mega-Crit").
    public override string Title => IsMegaUpgraded ? "Mega-Crit" : base.Title;

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            var vuln = enemy.GetPower<VulnerablePower>();
            var weak = enemy.GetPower<WeakPower>();
            if (vuln is { Amount: > 0 } || weak is { Amount: > 0 }) return false;
        }
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +4 damage (14 → 18). Mega: +7 more (18 → 25; StS1 M_UPGRADE_PLUS_ATTACK_DMG),
        // plus the "Mega-Crit" rename above.
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Damage.UpgradeValueBy(7m);
        }
    }
}
