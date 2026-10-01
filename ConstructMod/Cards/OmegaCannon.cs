using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class OmegaCannon : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(15m, ValueProp.Move)];

    public OmegaCannon() : base(5, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return EnergyHoverTip;
            yield return HoverTipFactory.FromPower<StrengthPower>(null);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Omega Cannon",
        "#Deal !Damage! damage.\nCosts 1 less [E] for each *Strength* you have.{IfUpgraded:show: Negative *Strength* also counts.|}");

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (card != this || Owner == null)
        {
            modifiedCost = originalCost;
            return false;
        }
        var str = Owner.Creature.Powers.OfType<StrengthPower>().FirstOrDefault()?.Amount ?? 0;
        if (!IsUpgraded && str < 0) str = 0;
        if (IsUpgraded) str = Math.Abs(str);
        modifiedCost = Math.Max(0m, originalCost - str);
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .WithHitFx("vfx/vfx_giant_horizontal_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
