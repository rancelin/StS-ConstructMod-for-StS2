using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Flamethrower : AbstractCycleCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromCard<Burn>();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(2m, ValueProp.Move)];

    public Flamethrower() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy,
        showInCardLibrary: false, autoAdd: false)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Flamethrower",
        "#Deal !Damage! damage to a random enemy for each *Burn* in your piles.{IfUpgraded:show:\\n*Cycle* if there are fewer than 2 *Burn*s.|}");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        if (!IsUpgraded) return false;
        return StatusCount() < 2;
    }

    private int StatusCount()
    {
        var player = Owner;
        return PileType.Draw.GetPile(player).Cards.Sum(c => c is Burn ? 1 : 0)
            + PileType.Hand.GetPile(player).Cards.Sum(c => c is Burn ? 1 : 0)
            + PileType.Discard.GetPile(player).Cards.Sum(c => c is Burn ? 1 : 0);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat) return;
        var hits = StatusCount();
        if (hits <= 0) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this, cardPlay)
            .TargetingRandomOpponents(combat)
            .WithHitFx("vfx/vfx_molten_fist").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
