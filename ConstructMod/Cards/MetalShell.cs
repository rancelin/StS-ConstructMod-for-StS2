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
    // 2-tier ladder: +1 = +1 power amount (stays Plated Armor), +2 = mega (power becomes
    // Metallicize, +1 Block). Matches the original MetalShell code (not its upgrade text).
    public override int IntrinsicMaxUpgradeLevel => 2;

    public override bool GainsBlock => true;

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            // Show the tooltip for whichever power this copy will grant (Metallicize only at mega).
            yield return IsMegaUpgraded
                ? HoverTipFactory.FromPower<MetallicizePower>(null)
                : HoverTipFactory.FromPower<PlatedArmorPower>(null);
        }
    }

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4m, ValueProp.Move),
        new PowerVar<PlatedArmorPower>("Plates", 2m),
        new PowerVar<MetallicizePower>("Metal", 2m)
    ];

    public MetalShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Metal Shell",
        "#Gain !Block! *Block*.\n{IfMega:mega:Gain {Metal} *Metallicize*.|Gain {Plates} *Plated Armor*.}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        // Original: un-upgraded grants Plated Armor; only the mega tier switches to Metallicize.
        if (IsMegaUpgraded)
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
        // Regular: power amount 2 → 3 (both vars kept in sync). Mega: Block 4 → 5.
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Plates"].UpgradeValueBy(1m);
            DynamicVars["Metal"].UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Block.UpgradeValueBy(1m);
        }
    }
}
