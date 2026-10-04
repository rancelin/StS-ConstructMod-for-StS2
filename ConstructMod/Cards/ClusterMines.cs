using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class ClusterMines : AbstractConstructCard
{
    private bool _discountActive;

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new DynamicVar("Hits", 3m)
    ];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public ClusterMines() : base(2, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Cluster Mines",
        "#Deal !Damage! damage to a random enemy {Hits} times.\nWhenever your *Block* is broken, this card costs 0{IfUpgraded:show: for this combat| until played}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat) return;
        var hits = (int)DynamicVars["Hits"].BaseValue;
        if (hits <= 0) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this, cardPlay)
            .TargetingRandomOpponents(combat)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        if (_discountActive)
        {
            _discountActive = false;
        }
    }

    public override Task AfterBlockBroken(PlayerChoiceContext choiceContext, Creature target, Creature? breaker)
    {
        if (target == Owner.Creature && !_discountActive)
        {
            _discountActive = true;
            if (IsUpgraded) EnergyCost.AddThisCombat(-9, reduceOnly: true);
            else EnergyCost.SetUntilPlayed(0, reduceOnly: true);
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
