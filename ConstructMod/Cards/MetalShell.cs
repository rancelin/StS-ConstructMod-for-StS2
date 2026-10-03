using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class MetalShell : AbstractConstructCard
{
    public override bool GainsBlock => true;

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            // Show the tooltip for whichever power this copy will grant.
            yield return IsUpgraded
                ? HoverTipFactory.FromPower<MetallicizePower>(null)
                : HoverTipFactory.FromPower<PlatedArmorPower>(null);
        }
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
        new PowerVar<PlatedArmorPower>("Plates", 3m),
        new PowerVar<MetallicizePower>("Metal", 3m)
    ];

    public MetalShell() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Metal Shell",
        "#Gain !Block! *Block*.{IfUpgraded:show:\nGain 3 *Metallicize*.|}\n{IfUpgraded:hide:Gain 3 *Plated Armor*.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        // Original StS1 Metal Shell: unupgraded grants Plated Armor, upgraded grants Metallicize.
        if (IsUpgraded)
        {
            await PowerCmd.Apply<MetallicizePower>(choiceContext, Owner.Creature,
                DynamicVars["Metal"].IntValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<PlatedArmorPower>(choiceContext, Owner.Creature,
                DynamicVars["Plates"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
