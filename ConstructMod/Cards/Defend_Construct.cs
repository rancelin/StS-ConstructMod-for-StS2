using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Defend_Construct : AbstractCycleCard
{
    // 2-tier ladder: +1 = +3 Block, +2 = mega (+6 more).
    public override int IntrinsicMaxUpgradeLevel => 2;

    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<DexterityPower>(null);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move)];

    public Defend_Construct() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Defend",
        "#*Cycle* if your *Dexterity* is negative.\nGain !Block! *Block*.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        var dexterity = Owner.Creature.GetPower<DexterityPower>();
        return dexterity is { Amount: < 0 };
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +3 Block (5 → 8). Mega: +6 more (8 → 14). StS1 Defend_Gold
        // UPGRADE_PLUS_BLOCK / M_UPGRADE_PLUS_BLOCK.
        DynamicVars.Block.UpgradeValueBy(CurrentUpgradeLevel == 1 ? 3m : 6m);
    }
}
