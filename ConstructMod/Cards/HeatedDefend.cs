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
    public override bool GainsBlock => true;
    protected override System.Collections.Generic.HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<DexterityPower>(null);
        }
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move)];

    public HeatedDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        Overheat = 5;
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Heated Defend",
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
        DynamicVars.Block.UpgradeValueBy(3m);
        // Original: upgradeOverheat(+5) on upgrade, then mega upgradeOverheat(+10).
        // Folded: +15 overheat (combined).
        UpgradeOverheat(15);
    }
}
