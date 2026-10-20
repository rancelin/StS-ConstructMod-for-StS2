using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class HeatedStrike : AbstractCycleCard
{
    // 2-tier ladder: +1 = +5 overheat, +2 = mega (+10 overheat AND +6 damage).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<StrengthPower>(null);
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6m, ValueProp.Move), new OverheatVar()];

    public HeatedStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        Overheat = 5;
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Heated Strike",
        "#*Cycle* if your *Strength* is negative.\nDeal !Damage! damage.\n*Overheat*: {Overheat}.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        var strength = Owner.Creature.GetPower<StrengthPower>();
        return strength is { Amount: < 0 };
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
        // Regular: +5 overheat. Mega: +10 overheat and +6 damage (StS1 M_UPGRADE_PLUS_ATTACK_DMG).
        if (CurrentUpgradeLevel == 1)
        {
            UpgradeOverheat(5);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            UpgradeOverheat(10);
            DynamicVars.Damage.UpgradeValueBy(6m);
        }
    }
}
