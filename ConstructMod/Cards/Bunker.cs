using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Bunker : AbstractConstructCard
{
    // 2-tier ladder: +1 = +1 Block, +2 = mega (Retain a random card at turn end).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BunkerPower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromKeyword(CardKeyword.Retain)
    ];

    public Bunker() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Bunker",
        "#Whenever a card is *Retained*, gain {BunkerPower} *Block*.{IfMega:mega:\nAt the end of your turn, *Retain* a random card.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BunkerPower>(choiceContext, Owner.Creature,
            DynamicVars["BunkerPower"].BaseValue, Owner.Creature, this);
        if (IsMegaUpgraded)
        {
            await PowerCmd.Apply<RetainRandomPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: +1 Block (2 → 3).
            DynamicVars["BunkerPower"].UpgradeValueBy(1m);
        }
        // Mega: no stat change — RetainRandom moves here (OnPlay, gated on IsMegaUpgraded).
    }
}
