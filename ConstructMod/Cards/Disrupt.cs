using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Disrupt : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>(null)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new DynamicVar("StrLoss", 2m)
    ];

    public Disrupt() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Disrupt",
        // NOTE: no "{IfMega:mega:Retain.}" here — the mega tier grants the Retain KEYWORD, and StS2
        // renders keyword badges itself, so spelling it out would show Retain twice. The leading '#'
        // opts into SimpleLoc markup (!Block!/!StrLoss!/*Block*); without it they print literally.
        "#Gain !Block! *Block*.\nEnemy loses !StrLoss! *Strength* for the rest of this turn.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (cardPlay.Target != null)
        {
            await PowerCmd.Apply<DisruptPower>(choiceContext, cardPlay.Target,
                DynamicVars["StrLoss"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: -1 more Strength (2 → 3). Mega: -1 more (3 → 4) and the card becomes
        // Retain (StS1 sets retain = true only in the mega tier).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["StrLoss"].UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["StrLoss"].UpgradeValueBy(1m);
            AddKeyword(CardKeyword.Retain);
        }
    }
}
