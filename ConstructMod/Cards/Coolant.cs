using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Coolant : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8m, ValueProp.Move), new DynamicVar("Cooling", 5m)];

    public Coolant() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        // No overheat (default -1) — Coolant modifies other cards' overheat, doesn't have its own.
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Coolant",
        "#Gain !Block! *Block*. Increase the *Overheat* threshold of all cards with *Overheat* by {Cooling}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        // Increase the Overheat field of every AbstractConstructCard with overheat > 0
        // across draw, hand, and discard by {Cooling}. (Per-combat modification; the field
        // is per-card-instance and combat resets rebuild from canonical copies.)
        var cooling = DynamicVars["Cooling"].IntValue;
        foreach (var pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            foreach (var c in pileType.GetPile(Owner).Cards.ToList())
            {
                if (c is AbstractConstructCard acc && acc.Overheat > 0)
                {
                    acc.Overheat += cooling;
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Original: upgrade grants Innate + new description (no stat change), then mega = +4 cooling.
        // Folded: +4 cooling + Innate.
        DynamicVars["Cooling"].UpgradeValueBy(4m);
        AddKeyword(CardKeyword.Innate);
    }
}
