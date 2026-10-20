using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Forcefield : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Retain;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(6m, ValueProp.Move)];

    public Forcefield() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Forcefield",
        "#Gain !Block! *Block*.{IfMega:mega:\nPut this on top of your draw pile.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        // StS1's mega tier sets rebound: the card returns to the top of the draw pile.
        if (IsMegaUpgraded)
        {
            await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +2 Block (6 → 8) and Innate. Mega: +3 more Block (8 → 11) and the card
        // rebounds to the top of the draw pile (StS1 M_UPGRADE_PLUS_GAIN_BLOCK / rebound).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Block.UpgradeValueBy(2m);
            AddKeyword(CardKeyword.Innate);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars.Block.UpgradeValueBy(3m);
        }
    }
}
