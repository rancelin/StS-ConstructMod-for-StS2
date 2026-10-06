using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class FlakBarrage : AbstractCycleCard
{
    // Cycle keyword (and thus its hover tip) only applies when upgraded, matching CanonicalKeywords below.
    // We deliberately do NOT chain through base.ExtraHoverTips here, because the base (AbstractCycleCard)
    // unconditionally adds the Cycle tip; we need it gated on IsUpgraded to match the keyword filter.
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (IsUpgraded)
            {
                yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
                yield return HoverTipFactory.FromPower<StrengthPower>(null);
            }
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords)
            {
                if (k != ConstructKeywords.Cycle || IsUpgraded) yield return k;
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(0m, ValueProp.Move),
        new DynamicVar("Hits", 4m)
    ];

    public FlakBarrage() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Flak Barrage",
        "#Deal !Damage! damage to a random enemy {Hits} times.{IfUpgraded:show:\\n*Cycle* if your *Strength* is 0 or less.|}");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        if (!IsUpgraded) return false;
        return !(Owner.Creature.GetPower<StrengthPower>()?.Amount > 0m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat) return;
        var hits = (int)DynamicVars["Hits"].BaseValue;
        if (hits <= 0) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this, cardPlay)
            .TargetingRandomOpponents(combat)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
