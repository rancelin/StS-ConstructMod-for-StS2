using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class GatlingGun : AbstractConstructCard
{
    protected override bool HasEnergyCostX => true;

    // 2-tier ladder: +1 = regular upgrade, +2 = mega (the original's tradeoff mega).
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(3m, ValueProp.Move), new DynamicVar("Shots", 2m)];

    public GatlingGun() : base(-1, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return EnergyHoverTip;
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Gatling Gun",
        "#Deal !Damage! damage to a random enemy {Shots} times for each [E] spent.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = ResolveEnergyXValue();
        if (x <= 0) return;
        var shots = x * DynamicVars["Shots"].IntValue;
        if (shots <= 0) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(shots).FromCard(this, cardPlay)
            .TargetingRandomOpponents(CombatState!)
            .WithHitFx("vfx/vfx_molten_fist")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (called once per UpgradeInternal, and
        // save/load replays the same order — so this reconstructs both tiers correctly).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: +1 damage per shot (3 → 4).
            DynamicVars.Damage.UpgradeValueBy(1m);
        }
        else
        {
            // Mega: -1 damage (4 → 3) but +1 shot per energy (2 → 3) — the original's tradeoff.
            DynamicVars.Damage.UpgradeValueBy(-1m);
            DynamicVars["Shots"].UpgradeValueBy(1m);
        }
    }
}
