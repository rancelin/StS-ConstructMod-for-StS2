using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Strike_Construct : AbstractCycleCard
{
    // 2-tier ladder: +1 = +3 damage, +2 = mega (+6 more).
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<StrengthPower>(null);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6m, ValueProp.Move)];

    public Strike_Construct() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Strike",
        "#*Cycle* if your *Strength* is negative.\nDeal !Damage! damage.");

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
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +3 damage (6 → 9). Mega: +6 more (9 → 15). StS1 Strike_Gold
        // UPGRADE_PLUS_ATTACK_DMG / M_UPGRADE_PLUS_ATTACK_DMG.
        DynamicVars.Damage.UpgradeValueBy(CurrentUpgradeLevel == 1 ? 3m : 6m);
    }
}
