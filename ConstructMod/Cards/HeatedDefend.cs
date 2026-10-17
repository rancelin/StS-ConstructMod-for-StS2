using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class HeatedDefend : AbstractCycleCard
{
    // 2-tier ladder: +1 = +5 overheat, +2 = mega (+10 overheat AND +6 Block).
    public override int IntrinsicMaxUpgradeLevel => 2;
    public override bool GainsBlock => true;
    protected override System.Collections.Generic.HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<DexterityPower>(null);
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new OverheatVar()];

    public HeatedDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        Overheat = 5;
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Heated Defend",
        "#*Cycle* if your *Dexterity* is negative.\nGain !Block! *Block*.\n*Overheat*: {Overheat}.");

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
        // Regular: +5 overheat. Mega: +10 overheat and +6 Block (StS1 M_UPGRADE_PLUS_BLOCK).
        if (CurrentUpgradeLevel == 1)
        {
            UpgradeOverheat(5);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            UpgradeOverheat(10);
            DynamicVars.Block.UpgradeValueBy(6m);
        }
    }
}
